using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using SpaceTimeWitch.Commands;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

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
        var psCards = PersonalSpaceCmd.GetCards(player)
            .Where(c => c.CurrentUpgradeLevel < c.MaxUpgradeLevel)
            .ToList();

        if (psCards.Count == 0) return;

        var rng = player.RunState.Rng.CombatCardGeneration;
        var toUpgrade = Math.Min(amt, psCards.Count);

        for (int i = 0; i < toUpgrade; i++)
        {
            var card = rng.NextItem(psCards);
            CardCmd.Upgrade(card);
            psCards.Remove(card);
        }
    }

}