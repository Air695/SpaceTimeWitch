using System.Linq;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Combat.AttackHits;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace SpaceTimeWitch.Extension.QG.WXC;

[RegisterPower]
public class SLWellwornParasolP : ModPowerTemplate, IAttackHitHookListener
{
    public override PowerType Type => PowerType.None;
    public override PowerStackType StackType => PowerStackType.Single;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"res://images/Extension/Powers/{GetType().Name}.png",
        BigIconPath: $"res://images/Extension/Powers/{GetType().Name}.png"
    );

    public Task BeforeAttackHit(AttackHitContext context) => Task.CompletedTask;

    public async Task AfterAttackHit(AttackHitContext context)
    {
        if (context.Dealer == null) return;
        if (!context.DamageProps.IsPoweredAttack()) return;

        // 找到持有者对应的伤害结果
        var myResult = context.Results.FirstOrDefault(r => r.Receiver == Owner);
        if (myResult == null) return;
        if (!myResult.WasFullyBlocked) return;

        var reflectDamage = myResult.BlockedDamage * 3;
        if (reflectDamage <= 0) return;

        await CreatureCmd.Damage(
            context.ChoiceContext,
            context.Dealer,
            reflectDamage,
            ValueProp.Unpowered,
            Owner,
            context.CardSource);
    }
}