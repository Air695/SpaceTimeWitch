using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace SpaceTimeWitch.Extension.MCJ.Card6;

[RegisterPower]
public class STWElytraP : ModPowerTemplate
{

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"res://images/Extension/Powers/{GetType().Name}.png",
        BigIconPath: $"res://images/Extension/Powers/{GetType().Name}.png"
    );

    public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        // 只影响攻击卡伤害，不影响能力/遗物等来源的伤害
        if (!props.IsPoweredAttack()) return 1m;
        // 减少拥有者受到的来自他人的伤害，每层 30%
        if (target == Owner && dealer != null && dealer != target)
            return Math.Max(0m, 1m - 0.30m * Amount);
        return 1m;
    }

}