using System.Linq;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Combat.AttackHits;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using SpaceTimeWitch.Extension.QG.WXC;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace SpaceTimeWitch.Extension.Powers;

[RegisterPower]
public class STWBleed : ModPowerTemplate, IAttackHitHookListener
{

    public override PowerType Type => PowerType.Debuff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"res://images/Extension/Powers/{GetType().Name}.png",
        BigIconPath: $"res://images/Extension/Powers/{GetType().Name}.png"
    );
    
    // ── 状态追踪 ──

    /// <summary>上回合是否造成过攻击伤害（初始 true 防止首回合误触发）</summary>
    private bool _attackedThisTurn = true;

    public override Task BeforeCombatStart()
    {
        _attackedThisTurn = true;
        return Task.CompletedTask;
    }

    // ── 每段攻击命中后（玩家/敌人通用，多段攻击每段都触发）──

    public Task BeforeAttackHit(AttackHitContext context) => Task.CompletedTask;

    public async Task AfterAttackHit(AttackHitContext context)
    {
        // 只有持有者本人造成的攻击伤害才触发
        if (context.Dealer != Owner) return;
        if (!context.DamageProps.IsPoweredAttack()) return;
        if (Amount <= 0 || !Owner.IsAlive) return;

        Flash();

        await CreatureCmd.Damage(
            context.ChoiceContext,
            Owner,
            Amount,
            ValueProp.Unpowered,
            dealer: null,
            cardSource: null);

        _attackedThisTurn = true;

        if (Owner.IsAlive && Amount > 0 && !HasSanguineDesire)
            await ReduceToTwoThirds();
    }

    // ── 回合开始：上回合未攻击 → 层数降为 2/3（玩家/敌人通用）──

    public override async Task AfterSideTurnStart(
        CombatSide side,
        IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        if (side != CombatSide.Player) return;
        if (Amount <= 0) return;

        if (!_attackedThisTurn && !HasSanguineDesire)
        {
            Flash();
            await ReduceToTwoThirds();
        }

        _attackedThisTurn = false;
    }

    private bool HasSanguineDesire =>
        Owner.Powers.OfType<SLSanguineDesireP>().Any();

    // ── 手动引爆（供 Individual 等外部机制调用）──

    /// <summary>
    /// 引爆目标身上的流血：造成等同于层数的伤害，然后层数降为 2/3。
    /// </summary>
    public static async Task Detonate(
        Creature target,
        PlayerChoiceContext ctx,
        Creature? dealer,
        CardModel? cardSource)
    {
        var bleed = target.Powers.OfType<STWBleed>().FirstOrDefault();
        if (bleed == null || bleed.Amount <= 0 || !target.IsAlive) return;

        var amount = bleed.Amount;

        await CreatureCmd.Damage(
            ctx, target, amount,
            ValueProp.Unblockable | ValueProp.Unpowered,
            dealer, cardSource);

        if (target.IsAlive && bleed.Amount > 0)
            await bleed.ReduceOrRemove();
    }

    private async Task ReduceOrRemove()
    {
        if (HasSanguineDesire) return;
        await ReduceToTwoThirds();
    }

    // ── 辅助 ──

    private async Task ReduceToTwoThirds()
    {
        int newAmount = (int)Math.Floor(Amount * 2m / 3m);
        int diff = (int)Amount - newAmount;
        if (diff <= 0) return;

        if (newAmount <= 0)
        {
            await PowerCmd.Remove(this);
        }
        else
        {
            for (int i = 0; i < diff; i++)
                await PowerCmd.Decrement(this);
        }
    }
}