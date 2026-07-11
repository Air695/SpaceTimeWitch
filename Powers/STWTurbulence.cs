using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using SpaceTimeWitch.Commands;
using SpaceTimeWitch.Relics;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace SpaceTimeWitch.Powers;

[RegisterPower]
public class STWTurbulence : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"res://images/SpaceTimeWitch/Powers/{GetType().Name}.png",
        BigIconPath: $"res://images/SpaceTimeWitch/Powers/{GetType().Name}.png"
    );

    private sealed class TrackData
    {
        /// <summary>被此能力消耗的卡牌 ID 集合（用于 10% 额外选项）</summary>
        public readonly HashSet<int> ConsumedIds = new();
    }

    protected override object InitInternalData() => new TrackData();

    public override async Task BeforeSideTurnStart(
    PlayerChoiceContext choiceContext, CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
{
    if (side != CombatSide.Player) return;
    var creature = Owner;
    if (creature == null) return;
    var player = creature.Player;
    if (player == null) return;

    var data = GetInternalData<TrackData>();
    var rng = player.RunState.Rng.Niche;

    // ── 1. 清理已不在消耗堆中的卡 ──
    var exhaustPile = PileType.Exhaust.GetPile(player);
    var aliveIds = exhaustPile.Cards.Select(c => c.Id.GetHashCode()).ToHashSet();
    data.ConsumedIds.RemoveWhere(id => !aliveIds.Contains(id));

    // ── 2. 消耗牌堆顶层数张卡（不够则洗入弃牌堆再消耗）──
    int count = Amount;
    var exhaustedThisTurn = new List<CardModel>();

    for (int i = 0; i < count; i++)
    {
        await CardPileCmd.ShuffleIfNecessary(choiceContext, player);
        var drawPile = PileType.Draw.GetPile(player);
        if (drawPile.Cards.Count == 0) break;

        var topCard = drawPile.Cards[^1]; // 牌堆顶部最后一张
        await CardCmd.Exhaust(choiceContext, topCard);
        data.ConsumedIds.Add(topCard.Id.GetHashCode());
        exhaustedThisTurn.Add(topCard);
    }

    if (exhaustedThisTurn.Count == 0) return;

    // ── 3. 获取当前激活的链接时空标签 ──
    var activeTags = player.Relics
        .OfType<ITagRelic>()
        .Select(r => r.AssociatedTag)
        .ToHashSet();

    if (activeTags.Count == 0) return;

    // ── 4. 每次消耗触发一次复现 ──
    for (int i = 0; i < exhaustedThisTurn.Count; i++)
    {
        var candidates = ModelDb.AllCardPools
            .SelectMany(p => p.GetUnlockedCards(
                player.UnlockState,
                player.RunState.CardMultiplayerConstraint))
            .Where(c => c.CanBeGeneratedInCombat
                        && c.Tags.Any(t => activeTags.Contains(t)))
            .DistinctBy(c => c.Id)
            .ToList();

        if (candidates.Count == 0) continue;

        var (cw, uw, rw) = WeightedCardSelectCmd.GetConfiguredWeights();
        var offered = WeightedCardSelectCmd.GenerateWeighted(
            player, candidates, count: 3,
            commonWeight: cw,
            uncommonWeight: uw,
            rareWeight: rw,
            rng: rng);

        // ── 5. 10% 概率：消耗堆中被此能力消耗的卡作为额外选项 ──
        var bonusPool = exhaustPile.Cards
            .Where(c => data.ConsumedIds.Contains(c.Id.GetHashCode()))
            .ToList();

        CardModel? bonusCard = null;
        if (bonusPool.Count > 0 && rng.NextFloat() < 0.1f)
        {
            bonusCard = rng.NextItem(bonusPool);
            exhaustPile.RemoveInternal(bonusCard, silent: true);
            exhaustPile.InvokeCardRemoveFinished();
            offered.Add(bonusCard);
        }

        var chosen = (await CardSelectCmd.FromSimpleGrid(
            choiceContext, offered, player,
            new CardSelectorPrefs(
                new LocString("cards", "STW_SHARED_CHOOSE_CARD"),
                minCount: 0, maxCount: 1)))
            .FirstOrDefault();

        if (chosen != null)
        {
            // 消耗堆取出的卡已在战斗中，直接移入手牌；新生成的卡走标准流程
            if (chosen == bonusCard)
                await CardPileCmd.Add(chosen, PileType.Hand);
            else
                await CardPileCmd.AddGeneratedCardToCombat(chosen, PileType.Hand, creator: chosen.Owner);
        }
        else if (bonusCard != null)
        {
            // 玩家跳过选择，bonus 卡放回消耗堆
            exhaustPile.AddInternal(bonusCard);
            exhaustPile.InvokeCardAddFinished();
        }
    }
}
}
