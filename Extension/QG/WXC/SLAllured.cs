using System.Linq;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Combat.AttackHits;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using SpaceTimeWitch.Extension.Powers;

namespace SpaceTimeWitch.Extension.QG.WXC;

[RegisterPower]
public class SLAllured : ModPowerTemplate, IAttackHitHookListener
{
    public override PowerType Type => PowerType.None;
    public override PowerStackType StackType => PowerStackType.Single;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"res://images/Extension/Powers/{GetType().Name}.png",
        BigIconPath: $"res://images/Extension/Powers/{GetType().Name}.png"
    );

    // ── 攻击伤害 +2 ──

    public override decimal ModifyDamageAdditive(
        Creature? target, decimal amount, ValueProp props,
        Creature? dealer, CardModel? cardSource)
    {
        if (dealer == Owner && props.IsPoweredAttack())
            return 2m;
        return 0m;
    }

    // ── 受到攻击时获得 2 层 STWBleed ──

    public Task BeforeAttackHit(AttackHitContext context) => Task.CompletedTask;

    public async Task AfterAttackHit(AttackHitContext context)
    {
        if (!context.Targets.Contains(Owner)) return;
        if (!context.DamageProps.IsPoweredAttack()) return;

        await PowerCmd.Apply<STWBleed>(
            context.ChoiceContext, Owner, 2, context.Dealer, context.CardSource);
    }

    // ── 玩家回合开始时移除（SLGlitterP 随后重新施加）──

    public override async Task BeforeSideTurnStart(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        if (side == CombatSide.Player)
            await PowerCmd.Remove(this);
    }
}