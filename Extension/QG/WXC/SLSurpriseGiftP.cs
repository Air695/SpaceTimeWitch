using System.Linq;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using SpaceTimeWitch.Powers;
using STS2RitsuLib.Combat.AttackHits;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace SpaceTimeWitch.Extension.QG.WXC;

[RegisterPower]
public class SLSurpriseGiftP : ModPowerTemplate, IAttackHitHookListener
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
        if (context.Dealer != Owner) return;
        if (!context.DamageProps.IsPoweredAttack()) return;

        // 已有存活的敌人持有 SLGift → 不重新施加
        if (CombatState.Enemies.Any(e =>
                e.IsAlive && e.Powers.OfType<SLGift>().Any()))
            return;

        // 给第一个存活的敌人目标施加 SLGift
        var target = context.Targets.FirstOrDefault(t => t.IsAlive);
        if (target == null) return;

        await PowerCmd.Apply<SLGift>(
            context.ChoiceContext, target, 1, Owner, context.CardSource);
    }
}