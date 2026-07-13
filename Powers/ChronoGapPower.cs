using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using SpaceTimeWitch.Commands;
using SpaceTimeWitch.Relics;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using MegaCrit.Sts2.Core.Entities.Powers;

namespace SpaceTimeWitch.Powers;

[RegisterPower]
public class ChronoGapPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"res://images/SpaceTimeWitch/Powers/{GetType().Name}.png",
        BigIconPath: $"res://images/SpaceTimeWitch/Powers/{GetType().Name}.png"
    );

    public override async Task BeforeSideTurnStart(
        PlayerChoiceContext choiceContext, CombatSide side,
        IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side != CombatSide.Player) return;
        var owner = Owner;
        if (owner == null) return;
        var player = owner.Player;
        if (player == null) return;

        // 遍历所有玩家，收集他们的标签遗物标签
        var allActiveTags = combatState.Players
            .SelectMany(p => p.Relics.OfType<ITagRelic>())
            .Select(r => r.AssociatedTag)
            .ToHashSet();

        if (allActiveTags.Count == 0) return;

        // 基于卡牌自身标签筛选池子（不再使用 OriginalCardTags）
        var pool = ModelDb.AllCardPools
            .SelectMany(p => p.GetUnlockedCards(
                player.UnlockState,
                player.RunState.CardMultiplayerConstraint))
            .Where(c => c.CanBeGeneratedInCombat
                        && c.Tags.Any(t => allActiveTags.Contains(t)))
            .DistinctBy(c => c.Id)
            .OrderBy(c => c.Id)
            .ToList();

        if (pool.Count == 0) return;

        // 加权随机选择 3 张候选卡
        var (cw, uw, rw) = WeightedCardSelectCmd.GetConfiguredWeights();
        var rng = player.RunState.Rng.CombatCardGeneration;
        var offered = WeightedCardSelectCmd.GenerateWeighted(
            player, pool, count: 3,
            commonWeight: cw, uncommonWeight: uw, rareWeight: rw, rng: rng);

        var prompt = new LocString("cards", "STW_DISCOVER_PROMPT");
        var chosen = (await CardSelectCmd.FromSimpleGrid(
            choiceContext, offered, player,
            new CardSelectorPrefs(prompt, minCount: 0, maxCount: 1)))
            .FirstOrDefault();

        if (chosen != null)
            await CardPileCmd.AddGeneratedCardToCombat(chosen, PileType.Hand, creator: chosen.Owner);
    }
}