using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using SpaceTimeWitch.Cards;

namespace SpaceTimeWitch.Extension.QG;

/// <summary>
/// 单次抽取请求：指定卡牌等级标签（可多个合并）、接口类型、所属卡池。
/// </summary>
public record DrawRequest(IReadOnlyList<CardTag> TierTags, Type InterfaceType, Type PoolType, string Label);

/// <summary>
/// 升级抽取的结果。
/// </summary>
public class LevelUpDrawResult
{
    /// <summary>升级是否成功</summary>
    public bool Success { get; set; }

    /// <summary>升级后的新等级（0 表示未成功）</summary>
    public int NewLevel { get; set; }

    /// <summary>计算出的情感区间</summary>
    public int Interval { get; set; }

    /// <summary>玩家选中的异想体卡牌列表</summary>
    public List<CardModel> ChosenAbnormalities { get; set; } = [];

    /// <summary>玩家选中的EGO卡牌列表</summary>
    public List<CardModel> ChosenEGOs { get; set; } = [];
}

/// <summary>
/// 情感抽取系统。在情感升级时触发，根据情感区间和HP修正从异想体/EGO卡池中三选一抽取。
/// </summary>
public static class EmotionDrawSystem
{
    private static readonly HashSet<ModelId> s_chosenThisCombat = [];

    /// <summary>清除本场已选取记录（战斗开始时调用）</summary>
    public static void ResetChosenCards() => s_chosenThisCombat.Clear();

    // ─── 可配置参数 ───

    /// <summary>HP=100% 时的乘数</summary>
    public static double HpFullMultiplier = 2.0;

    /// <summary>HP=1% 时的除数（情感值除以此值）</summary>
    public static double HpLowDivisor = 3.0;

    /// <summary>每次抽取展示的卡牌数量</summary>
    public const int OfferCount = 3;

    // ─── 情感计算 ───

    /// <summary>
    /// 根据正面/负面情感值和HP比例计算情感区间（-2 ~ 2）。
    /// </summary>
    /// <param name="positive">正面情感值</param>
    /// <param name="negative">负面情感值</param>
    /// <param name="hpRatio">当前HP比例 (0~1)</param>
    public static int CalculateInterval(int positive, int negative, double hpRatio)
    {
        var total = positive + negative;
        if (total <= 0) return 0;

        var raw = (double)positive / total * 100.0;

        // 线性插值：HP=1%时除 HpLowDivisor，HP=100%时乘 HpFullMultiplier
        var lowMult = 1.0 / HpLowDivisor;
        var t = (hpRatio - 0.01) / 0.99;
        if (t < 0) t = 0;
        if (t > 1) t = 1;
        var multiplier = lowMult + t * (HpFullMultiplier - lowMult);

        var adjusted = raw * multiplier;
        adjusted = Math.Clamp(adjusted, 0, 100);

        return adjusted switch
        {
            < 20 => -2,
            < 40 => -1,
            < 60 => 0,
            < 80 => 1,
            _ => 2,
        };
    }

    // ─── 排序逻辑 ───

    /// <summary>
    /// 按区间距离排序异想体卡牌列表。
    /// HP≥50% 优先级: dist=0, +1, -1, +2, -2
    /// HP&lt;50% 优先级: dist=0, -1, +1, -2, +2
    /// </summary>
    public static List<CardModel> SortByInterval(
        List<CardModel> abnormalities, int playerInterval, bool hpAboveHalf)
    {
        int[] priority = hpAboveHalf
            ? [0, 1, -1, 2, -2]
            : [0, -1, 1, -2, 2];

        var rank = new Dictionary<int, int>();
        for (var i = 0; i < priority.Length; i++)
            rank[priority[i]] = i;

        var rng = new Random();

        return abnormalities
            .Select(c => (
                card: c,
                dist: ((IAbnormalityCard)c).Interval - playerInterval
            ))
            .OrderBy(x => rank.GetValueOrDefault(x.dist, int.MaxValue))
            .ThenBy(_ => rng.Next())
            .Select(x => x.card)
            .ToList();
    }

    // ─── 标签提取 ───

    /// <summary>
    /// 从玩家的 QG 遗物中获取所有异想体标签（排除 WXC 自身）。
    /// </summary>
    private static HashSet<CardTag> GetActiveAbnormalityTags(Player player)
    {
        return player.Relics
            .OfType<IQGRelic>()
            .SelectMany(r => r.AbnormalityTags)
            .Where(t => t != CardTags.WXC)
            .ToHashSet();
    }

    // ─── 卡牌筛选 ───

    /// <summary>
    /// 从指定卡池中筛选匹配标签和接口类型的卡牌（原型）。
    /// </summary>
    private static List<CardModel> FilterCards(
        Player player, HashSet<CardTag> abnormalityTags, DrawRequest request)
    {
        var targetPool = ModelDb.AllCardPools
            .FirstOrDefault(p => p.GetType() == request.PoolType);
        if (targetPool == null) return [];

        var pool = targetPool
            .GetUnlockedCards(player.UnlockState, player.RunState.CardMultiplayerConstraint)
            .Where(c => c.CanBeGeneratedInCombat
                        && c.Tags.Any(t => abnormalityTags.Contains(t) || request.TierTags.Contains(t))
                        && request.InterfaceType.IsInstanceOfType(c))
            .DistinctBy(c => c.Id)
            .ToList();

        return pool;
    }

    // ─── 单次抽取 ───

    private static async Task<CardModel?> DoDraw(
        PlayerChoiceContext? ctx,
        Player player,
        IReadOnlyList<CardModel> candidates)
    {
        if (candidates.Count == 0) return null;
        if (ctx == null) return null;

        var count = Math.Min(candidates.Count, OfferCount);
        var offered = candidates.Take(count).ToList();

        var combatCards = offered
            .Select(c => player.Creature.CombatState.CreateCard(c, player))
            .ToList();

        return await CardSelectCmd.FromChooseACardScreen(ctx, combatCards, player, canSkip: true);
    }

    // ─── 主入口 ───

    /// <summary>
    /// 升级后触发抽取。调用方应先执行 TryLevelUp，然后传入升级前的情感值。
    /// </summary>
    public static async Task<LevelUpDrawResult> PerformDraw(
        PlayerChoiceContext? ctx, Player player,
        int oldLevel, int prePositive, int preNegative)
    {
        var result = new LevelUpDrawResult();

        var newLevel = EmotionSystem.GetLevel(player);
        result.Success = true;
        result.NewLevel = newLevel;

        // 3. 合并所有情感遗物的标签族 → 每级固定抽取（4、5 级额外抽 EGO）
        var allRelics = player.Relics.OfType<QGRelicBase>().ToList();
        var t1 = allRelics.Select(r => r.Tier1Tag).Distinct().ToList();
        var t2 = allRelics.Select(r => r.Tier2Tag).Distinct().ToList();
        var t3 = allRelics.Select(r => r.Tier3Tag).Distinct().ToList();
        var eg = allRelics.Select(r => r.EGOTag).Distinct().ToList();

        var tierTags = newLevel switch
        {
            1 or 2 => t1,
            3 or 4 => t2,
            5 => t3,
            _ => []
        };
        if (tierTags.Count == 0) return result;

        var requests = new List<DrawRequest>
        {
            new(tierTags, typeof(IAbnormalityCard), typeof(STWLRA), $"L{newLevel}")
        };
        if (newLevel is 4 or 5 && eg.Count > 0)
            requests.Add(new(eg, typeof(IEGOCard), typeof(STWEGO), "EGO"));

        var abnormalityTags = GetActiveAbnormalityTags(player);

        // 4. 计算情感区间
        var hpRatio = player.Creature.MaxHp > 0
            ? (double)player.Creature.CurrentHp / player.Creature.MaxHp
            : 0.5;
        var interval = CalculateInterval(prePositive, preNegative, hpRatio);
        result.Interval = interval;

        var hpAboveHalf = hpRatio >= 0.5;

        if (requests.Count == 0) return result;

        // 6. 执行每次抽取
        foreach (var request in requests)
        {
            var isAbnormality = request.InterfaceType == typeof(IAbnormalityCard);

            // 筛选候选卡牌（排除本场已选取的）
            var allCandidates = FilterCards(player, abnormalityTags, request);
            var candidates = allCandidates
                .Where(c => !s_chosenThisCombat.Contains(c.Id))
                .ToList();

            IReadOnlyList<CardModel> orderedCandidates = candidates;

            // 异想体按区间排序
            if (isAbnormality)
                orderedCandidates = SortByInterval(candidates, interval, hpAboveHalf);

            CardModel? chosen = await DoDraw(ctx, player, orderedCandidates);
            Scripts.Entry.Logger.Info(
                $"Draw level={newLevel}: chosen={chosen?.Id}");

            if (chosen == null) continue;

            s_chosenThisCombat.Add(chosen.Id);

            // 应用效果
            if (isAbnormality && chosen is IAbnormalityCard abnormality)
            {
                await abnormality.ApplyEffect(ctx, player);
                result.ChosenAbnormalities.Add(chosen);
            }
            else if (chosen is IEGOCard)
            {
                await CardPileCmd.AddGeneratedCardToCombat(chosen, PileType.Hand,
                    creator: player);
                result.ChosenEGOs.Add(chosen);
            }
        }

        return result;
    }
}
