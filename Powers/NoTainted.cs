using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models;

namespace SpaceTimeWitch.Powers;

/// <summary>
/// 受到 TaintedPower 时只被施加 1/2 层数。
/// </summary>
[RegisterPower]
public class NoTainted : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"res://images/SpaceTimeWitch/Powers/No.png",
        BigIconPath: $"res://images/SpaceTimeWitch/Powers/No.png"
    );

    public override bool TryModifyPowerAmountReceived(
        PowerModel canonicalPower, Creature target, decimal amount,
        Creature? applier, out decimal modifiedAmount)
    {
        modifiedAmount = amount;
        if (canonicalPower is not TaintedPower) return false;
        if (target != Owner) return false;
        if (amount <= 0) return false;

        modifiedAmount = (int)(amount / 2);
        return true;
    }
}