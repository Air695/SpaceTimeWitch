using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using SpaceTimeWitch.Field;
using STS2RitsuLib.Interop.AutoRegistration;

namespace SpaceTimeWitch.Cards.Fields;

[RegisterPower]
public class GravityFieldPower : FieldPowerBase
{
    public override FieldBackgroundType BackgroundType => FieldBackgroundType.Replace;

    public override string BackgroundPath => "res://images/SpaceTimeWitch/Field/GravityFieldBg.png";
    
    public override decimal ModifyDamageMultiplicative(
        Creature? target, decimal amount, ValueProp props,
        Creature? dealer, CardModel? cardSource)
    {
        if (dealer != Owner) return 1m;
        if (!props.IsPoweredAttack()) return 1m;
        return 0.75m;
    }
}