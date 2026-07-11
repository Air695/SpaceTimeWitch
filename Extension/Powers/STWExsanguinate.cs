using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Combat.AttackHits;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using SpaceTimeWitch.Extension.Powers;

namespace SpaceTimeWitch.Extension.Powers;

[RegisterPower]
public class STWExsanguinate : ModPowerTemplate, IAttackHitHookListener
{

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"res://images/Extension/Powers/{GetType().Name}.png",
        BigIconPath: $"res://images/Extension/Powers/{GetType().Name}.png"
    );

    public Task BeforeAttackHit(AttackHitContext context) => Task.CompletedTask;

    public async Task AfterAttackHit(AttackHitContext context)
    {
        if (context.Dealer != Owner) return;
        if (!context.DamageProps.IsPoweredAttack()) return;
        if (Amount <= 0) return;

        foreach (var result in context.Results)
        {
            if (result.UnblockedDamage > 0 && result.Receiver.IsAlive)
            {
                await PowerCmd.Apply<STWBleed>(
                    context.ChoiceContext, result.Receiver,
                    (int)Amount, Owner, context.CardSource);
            }
        }
    }
}