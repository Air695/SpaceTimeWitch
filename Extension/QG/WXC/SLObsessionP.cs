using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using SpaceTimeWitch.Extension.Powers;
using MegaCrit.Sts2.Core.Models;

namespace SpaceTimeWitch.Extension.QG.WXC;

[RegisterPower]
public class SLObsessionP : ModPowerTemplate
{
    public override PowerType Type => PowerType.None;
    public override PowerStackType StackType => PowerStackType.Single;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"res://images/Extension/Powers/XX.png",
        BigIconPath: $"res://images/Extension/Powers/XX.png"
    );

    /// <summary>
    /// 记录上一次翻倍对应的 (能力模板, 目标, 施加者)。
    /// 同一组 key 的连续调用只翻倍一次，防止多个 SLObsessionP 实例叠加。
    /// </summary>
    private static (PowerModel? power, Creature? target, Creature? applier) _lastDoubled;

    public override bool TryModifyPowerAmountReceived(
        PowerModel canonicalPower,
        Creature target,
        decimal amount,
        Creature? applier,
        out decimal modifiedAmount)
    {
        if (canonicalPower is STWBleed)
        {
            var key = (canonicalPower, target, applier);
            if (_lastDoubled != key)
            {
                _lastDoubled = key;
                modifiedAmount = amount * 2;
                return true;
            }
        }

        modifiedAmount = amount;
        return false;
    }
}