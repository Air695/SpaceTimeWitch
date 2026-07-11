using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace SpaceTimeWitch.Powers;

[RegisterPower]
public class Hyperdimensional : ModPowerTemplate
{

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"res://images/SpaceTimeWitch/Powers/{GetType().Name}.png",
        BigIconPath: $"res://images/SpaceTimeWitch/Powers/{GetType().Name}.png"
    );

    private const decimal BuffMultiplier = 1.273m;
    private const decimal DebuffMultiplier = 0.727m;
    // 追踪小数部分累积（跨能量获取事件持久化），攒够1点即发放
    private sealed class EnergyTracker
    {
        public decimal Fraction;
    }

    protected override object InitInternalData() => new EnergyTracker();

    public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (target == Owner && props.IsPoweredAttack())
            return DebuffMultiplier;
        if (dealer == Owner && props.IsPoweredAttack())
            return BuffMultiplier;
        return 1m;
    }

    // 格挡 ×1.273
    public override decimal ModifyBlockMultiplicative(Creature target, decimal block, ValueProp props, CardModel? cardSource, CardPlay? cardPlay)
    {
        if (target == Owner && props.IsPoweredCardOrMonsterMoveBlock())
            return BuffMultiplier;
        return 1m;
    }

    // 能量获取：×1.273 累积制 — 小数部分不会丢失，攒够整数即发放
    public override decimal ModifyEnergyGain(Player player, decimal amount)
    {
        if (player != Owner.Player)
            return amount;

        var tracker = GetInternalData<EnergyTracker>();
        decimal raw = amount * BuffMultiplier + tracker.Fraction;
        decimal gained = Math.Floor(raw);
        tracker.Fraction = raw - gained;
        return gained;
    }
    
    // 失去生命 ×0.727
    public override decimal ModifyHpLostAfterOsty(Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (target == Owner)
            return amount * DebuffMultiplier;
        return amount;
    }

    // 正面buff层数 ×1.273, 负面buff层数 ×0.727
    public override bool TryModifyPowerAmountReceived(PowerModel canonicalPower, Creature target, decimal amount, Creature? applier, out decimal modifiedAmount)
    {
        if (target != Owner)
        {
            modifiedAmount = amount;
            return false;
        }

        if (canonicalPower.GetTypeForAmount(amount) == PowerType.Buff)
        {
            modifiedAmount = amount * BuffMultiplier;
            return true;
        }

        if (canonicalPower.GetTypeForAmount(amount) == PowerType.Debuff)
        {
            modifiedAmount = amount * DebuffMultiplier;
            return true;
        }
        
        if (canonicalPower.GetTypeForAmount(amount) == PowerType.Debuff)
        {
            decimal newAmount = amount * DebuffMultiplier;
            // 如果修正后层数 ≤ 0，则完全阻止此能力被施加
            if (newAmount <= 0)
            {
                modifiedAmount = 0;
                return false;  // 阻止应用
            }
            modifiedAmount = newAmount;
            return true;
        }

        modifiedAmount = amount;
        return false;
    }

}