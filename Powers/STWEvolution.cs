using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using SpaceTimeWitch.Commands;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using MegaCrit.Sts2.Core.Entities.Cards;

namespace SpaceTimeWitch.Powers;

[RegisterPower]
public class STWEvolution : ModPowerTemplate
{

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"res://images/SpaceTimeWitch/Powers/{GetType().Name}.png",
        BigIconPath: $"res://images/SpaceTimeWitch/Powers/{GetType().Name}.png"
    );
    
    public override async Task BeforeSideTurnStart(
        PlayerChoiceContext choiceContext, CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side != CombatSide.Player) return;

        var owner = Owner;
        if (owner == null) return;

        var player = owner.Player;
        if (player == null) return;

        var amt = Amount;

        var handCards = PileType.Hand.GetPile(player).Cards;
        var drawCards = PileType.Draw.GetPile(player).Cards;
        var discardCards = PileType.Discard.GetPile(player).Cards;
        var psCards = PersonalSpaceCmd.GetCards(player);

        var allCards = handCards
            .Concat(drawCards)
            .Concat(discardCards)
            .Concat(psCards)
            .Where(c => c.CurrentUpgradeLevel < c.MaxUpgradeLevel)
            .OrderBy(c => c.Id)
            .ToList();

        if (allCards.Count == 0) return;

        var rng = player.RunState.Rng.CombatCardGeneration;
        var toUpgrade = Math.Min(amt, allCards.Count);

        for (int i = 0; i < toUpgrade; i++)
        {
            var card = rng.NextItem(allCards);
            CardCmd.Upgrade(card);
            allCards.Remove(card);
        }
    }

}