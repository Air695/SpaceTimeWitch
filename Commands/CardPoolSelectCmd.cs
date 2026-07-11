using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Cards;
using SpaceTimeWitch.Character;
using SpaceTimeWitch.Extension.QG;
using SpaceTimeWitch.Cards.Show;

namespace SpaceTimeWitch.Commands;

public static class CardPoolSelectCmd
{
    // ── 排除的卡池类型 ──
    private static readonly HashSet<Type> ExcludedPoolTypes =
    [
        typeof(CurseCardPool),
        typeof(StatusCardPool),
        typeof(TokenCardPool),
        typeof(DeprecatedCardPool),
        typeof(EventCardPool),
        typeof(QuestCardPool),
        typeof(MockCardPool),
        typeof(STWLRA),
        typeof(STWYC),
    ];

    // ── 预设代表卡映射 ──
    private static readonly IReadOnlyDictionary<Type, Func<Player, CardModel>> PoolReps =
        new Dictionary<Type, Func<Player, CardModel>>
        {
            [typeof(IroncladCardPool)]    = p => CreateRep<STWSIronclad>(p),
            [typeof(SilentCardPool)]      = p => CreateRep<STWSSilent>(p),
            [typeof(DefectCardPool)]      = p => CreateRep<STWSDefect>(p),
            [typeof(NecrobinderCardPool)] = p => CreateRep<STWSNecrobinder>(p),
            [typeof(RegentCardPool)]      = p => CreateRep<STWSRegent>(p),
            [typeof(ColorlessCardPool)]   = p => CreateRep<STWSColorLoss>(p),
            [typeof(SpaceTimeWitchCardPool)]   = p => CreateRep<STWSSTW>(p),
            [typeof(SpaceTimeWitchExCardPool)]   = p => CreateRep<STWSEX>(p),
            [typeof(STWEGO)]   = p => CreateRep<STWSEGO>(p),
        };

    private static CardModel CreateRep<T>(Player player) where T : CardModel
    {
        var card = ModelDb.GetById<CardModel>(ModelDb.GetId<T>()).ToMutable();
        card.Owner = player;
        return card;
    }

    // ── 原版五角色卡池（Ironclad、Silent、Defect、Regent、Necrobinder） ──
    public static readonly IReadOnlyList<CardPoolModel> OriginalCharacterPools =
    [
        ModelDb.CardPool<IroncladCardPool>(),
        ModelDb.CardPool<SilentCardPool>(),
        ModelDb.CardPool<DefectCardPool>(),
        ModelDb.CardPool<RegentCardPool>(),
        ModelDb.CardPool<NecrobinderCardPool>(),
    ];

    /// <summary>
    /// 获取所有非衍生卡池的列表。
    /// </summary>
    public static IReadOnlyList<CardPoolModel> GetAllowedPools() =>
        ModelDb.AllCardPools
            .Where(p => !ExcludedPoolTypes.Contains(p.GetType()))
            .ToList();

    // ═══════════════════════════════════════════
    // 选择卡池（自动代表卡）
    // ═══════════════════════════════════════════

    /// <summary>
    /// 从给定的卡池列表中让玩家选择一个卡池。
    /// 代表卡优先级：预设映射 → 打击卡(Basic+Attack) → 首张普通卡 → 首张任意卡。
    /// </summary>
    public static async Task<CardPoolModel?> SelectPool(
        PlayerChoiceContext context,
        Player player,
        IEnumerable<CardPoolModel> pools,
        LocString? prompt = null)
    {
        var poolReps = new List<(CardPoolModel pool, CardModel rep)>();

        foreach (CardPoolModel pool in pools)
        {
            if (poolReps.Any(p => p.pool == pool))
                continue;

            CardModel? rep = FindBestRep(pool, player);
            if (rep != null)
                poolReps.Add((pool, rep));
        }

        return await SelectPoolInternal(context, player, poolReps, prompt);
    }

    // ═══════════════════════════════════════════
    // 选择卡池（自定义代表卡）
    // ═══════════════════════════════════════════

    /// <summary>
    /// 从带有自定义代表卡的卡池列表中选择一个卡池。
    /// 调用方完全控制每个卡池展示什么卡牌。
    /// </summary>
    public static async Task<CardPoolModel?> SelectPool(
        PlayerChoiceContext context,
        Player player,
        IEnumerable<(CardModel Rep, CardPoolModel Pool)> delegates,
        LocString? prompt = null)
    {
        var poolReps = new List<(CardPoolModel pool, CardModel rep)>();
        var addedPools = new HashSet<CardPoolModel>();

        foreach (var (rep, pool) in delegates)
        {
            if (rep == null || pool == null || addedPools.Contains(pool))
                continue;
            poolReps.Add((pool, rep));
            addedPools.Add(pool);
        }

        return await SelectPoolInternal(context, player, poolReps, prompt);
    }

    // ═══════════════════════════════════════════
    // 内部实现
    // ═══════════════════════════════════════════

    private static CardModel? FindBestRep(CardPoolModel pool, Player player)
    {
        var unlocked = pool
            .GetUnlockedCards(player.UnlockState, player.RunState.CardMultiplayerConstraint)
            .ToList();
        if (unlocked.Count == 0)
            return null;

        // 1. 预设代表卡
        if (PoolReps.TryGetValue(pool.GetType(), out var factory))
            return factory(player);

        // 2. 打击卡（Basic + Attack）
        var strike = unlocked.FirstOrDefault(c =>
            c.Rarity == CardRarity.Basic && c.Type == CardType.Attack);
        if (strike != null)
            return strike;

        // 3. 首张普通卡
        var common = unlocked.FirstOrDefault(c => c.Rarity == CardRarity.Common);
        if (common != null)
            return common;

        // 4. 首张任意卡
        return unlocked.FirstOrDefault();
    }

    private static async Task<CardPoolModel?> SelectPoolInternal(
        PlayerChoiceContext context,
        Player player,
        IReadOnlyList<(CardPoolModel pool, CardModel rep)> poolReps,
        LocString? prompt)
    {
        if (poolReps.Count == 0)
            return null;

        if (poolReps.Count == 1)
            return poolReps[0].pool;

        var reps = poolReps.Select(p => p.rep).ToList();
        var prefs = new CardSelectorPrefs(
            prompt ?? new LocString("card_selection", "CHOOSE_POOL"),
            0, 1);

        IEnumerable<CardModel> selected = await MegaCrit.Sts2.Core.Commands.CardSelectCmd.FromSimpleGrid(
            context, reps, player, prefs);

        CardModel? chosen = selected.FirstOrDefault();
        if (chosen == null)
            return null;

        return poolReps.First(p => p.rep == chosen).pool;
    }
}
