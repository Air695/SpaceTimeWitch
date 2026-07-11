using System.Linq;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Combat.AttackHits;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using SpaceTimeWitch.Extension.Powers;

namespace SpaceTimeWitch.Extension.QG.WXC;

[RegisterPower]
public class SLAxeP : ModPowerTemplate, IAttackHitHookListener
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

        // ① 每个目标施加 2 流血
        foreach (var target in context.Targets)
        {
            await PowerCmd.Apply<STWBleed>(
                context.ChoiceContext, target, 2, Owner, context.CardSource);
        }

        // ② 根据格挡情况给自身施加流血
        var results = context.Results;
        if (results.Count == 0) return;

        if (results.Any(r => r.WasFullyBlocked))
        {
            // 完全格挡 → 自身 2 流血
            await PowerCmd.Apply<STWBleed>(
                context.ChoiceContext, Owner, 2, context.Dealer, context.CardSource);
        }
        else if (results.Any(r => r.BlockedDamage > 0))
        {
            // 部分格挡 → 自身 1 流血
            await PowerCmd.Apply<STWBleed>(
                context.ChoiceContext, Owner, 1, context.Dealer, context.CardSource);
        }
    }
}