using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using SpaceTimeWitch.Extension.Powers;

namespace SpaceTimeWitch.Extension.Powers;

[RegisterPower]
public class STWBleedN : ModPowerTemplate
{

    public override PowerType Type => PowerType.Debuff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"res://images/Extension/Powers/STWBleed.png",
        BigIconPath: $"res://images/Extension/Powers/STWBleed.png"
    );

    public override async Task AfterSideTurnStart(
        CombatSide side,
        IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        if (side != CombatSide.Player) return;
        if (Amount <= 0 || !Owner.IsAlive) return;

        var bleedAmount = (int)Amount;
        await PowerCmd.Remove(this);
        await PowerCmd.Apply<STWBleed>(
            new ThrowingPlayerChoiceContext(), Owner,
            bleedAmount, Owner, null);
    }
}