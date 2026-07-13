using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using MegaCrit.Sts2.Core.Models;

namespace SpaceTimeWitch.Powers;

/// <summary>
/// 免疫 Smoggy：阻止 SmoggyPower 施加到持有者。
/// </summary>
[RegisterPower]
public class NoSmoggy : ModPowerTemplate
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
        if (canonicalPower is not SmoggyPower) return false;
        if (target != Owner) return false;
        if (amount <= 0) return false;

        modifiedAmount = 0;
        Flash();
        return true;
    }
}