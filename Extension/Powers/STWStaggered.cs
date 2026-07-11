using System.Linq;
using System.Threading.Tasks;
using STS2RitsuLib.Combat.AttackHits;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace SpaceTimeWitch.Extension.Powers;

[RegisterPower]
public class STWStaggered : ModPowerTemplate, IAttackHitHookListener
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Single;

    public Task BeforeAttackHit(AttackHitContext context) => Task.CompletedTask;

    public async Task AfterAttackHit(AttackHitContext context)
    {
        // 只在最后一段命中后移除，避免过早移除导致后续段数吃不到加成
        if (context.HitIndex != context.TotalHitCount - 1) return;

        foreach (var result in context.Results)
        {
            var target = result.Receiver;
            if (!target.IsAlive) continue;

            var staggered = target.Powers.OfType<STWStaggered>().FirstOrDefault();
            if (staggered != null)
                await PowerCmd.Remove(staggered);
        }
    }

    /// <summary>伤害 ×1.5</summary>
    public override decimal ModifyDamageMultiplicative(
        Creature? target, decimal amount, ValueProp props,
        Creature? dealer, CardModel? cardSource)
    {
        if (target != Owner) return 1m;
        if (!props.IsPoweredAttack()) return 1m;
        return 1.5m;
    }

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"res://images/Extension/Powers/{GetType().Name}.png",
        BigIconPath: $"res://images/Extension/Powers/{GetType().Name}.png"
    );
}
