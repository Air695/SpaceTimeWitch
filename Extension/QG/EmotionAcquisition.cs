using System.Linq;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Combat.AttackHits;
using STS2RitsuLib.Combat.SecondaryResources;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace SpaceTimeWitch.Extension.QG;

/// <summary>
/// 情感获取 — 战斗中累积计数，玩家回合开始时统一结算：先正面后负面。
/// </summary>
[RegisterPower]
public class QGEmotionAcquisition : ModPowerTemplate, IAttackHitHookListener
{
    public override PowerType Type => PowerType.None;
    public override PowerStackType StackType => PowerStackType.Single;

    private int _positiveCount;
    private int _negativeCount;
    private int _unblockedHitCount;
    private int _fullyBlockedHitCount;
    private bool _dealtUnblockedDmg;

    // 追踪单个敌人单次攻击行动的完全格挡
    private Creature? _currentAttacker;
    private int _currentAttackTotalHits;
    private int _currentAttackFullyBlockedHits;
    private bool _combatStarted;

    private static void Log(string msg) => Scripts.Entry.Logger.Info(msg);
    
    protected override bool IsVisibleInternal => false;

    // ═══════════════════ 回合开始：先正面后负面 ═══════════════════

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext _, Player player)
    {
        if (Owner == null || !EmotionSystem.HasQGRelic(player)) return;

        if (_combatStarted)
        {
            Log($"TurnStart apply: positive={_positiveCount} negative={_negativeCount}");
            // 先正面
            if (_positiveCount > 0)
            {
                await EmotionSystem.AddPositive(player, _positiveCount);
            }

            // 后负面
            if (_negativeCount > 0)
            {
                await EmotionSystem.AddNegative(player, _negativeCount);
            }
        }
        _combatStarted = true;

        // 重置
        _positiveCount = 0;
        _negativeCount = 0;
        _unblockedHitCount = 0;
        _fullyBlockedHitCount = 0;
        _dealtUnblockedDmg = false;
        FlushFullBlock();
    }

    // ═══════════════════ 伤害管道 ═══════════════════

    public override decimal ModifyDamageAdditive(
        Creature? target, decimal amount, ValueProp props,
        Creature? dealer, CardModel? cardSource)
    {
        if (target == null || dealer == null || Owner == null) return 0m;
        if (!props.IsPoweredAttack()) return 0m;
        var player = Owner.Player;
        if (player == null || !EmotionSystem.HasQGRelic(player)) return 0m;

        // 攻击卡造成穿透伤害（每卡最多+1，在AfterCardPlayed处理）
        if (dealer == Owner && target != Owner && amount > 0
            && cardSource != null && cardSource.Type == CardType.Attack)
            _dealtUnblockedDmg = true;

        return 0m;
    }

    // ═══════════════════ 攻击命中：受击计数（仅实际伤害，排除预览）═══════════════════

    public Task BeforeAttackHit(AttackHitContext context) => Task.CompletedTask;

    public async Task AfterAttackHit(AttackHitContext context)
    {
        if (Owner == null) return;
        var player = Owner.Player;
        if (player == null || !EmotionSystem.HasQGRelic(player)) return;

        // 被怪物攻击命中
        if (context.Targets.Contains(Owner) && context.Dealer != Owner
            && context.Dealer.IsMonster && context.Damage > 0)
        {
            // 追踪单个敌人单次攻击行动的完全格挡
            if (context.Dealer != _currentAttacker)
            {
                // 新攻击者：结算上一攻击者的完全格挡
                FlushFullBlock();
                _currentAttacker = context.Dealer;
                _currentAttackTotalHits = (int)context.TotalHitCount;
                _currentAttackFullyBlockedHits = 0;
            }

            foreach (var result in context.Results.Where(r => r.Receiver == Owner))
            {
                if (result.WasFullyBlocked)
                    _currentAttackFullyBlockedHits++;
                else if (result.BlockedDamage < result.TotalDamage)
                    _unblockedHitCount++;
            }

            // 最后一段：检查整次攻击是否完全格挡
            if (context.HitIndex >= _currentAttackTotalHits - 1)
                FlushFullBlock();
        }
    }

    // ═══════════════════ 卡牌打出 ═══════════════════

    public override async Task AfterCardPlayed(PlayerChoiceContext _, CardPlay cardPlay)
    {
        if (Owner == null) return;
        var player = cardPlay.Card?.Owner;
        if (player?.Creature != Owner) return;
        if (!EmotionSystem.HasQGRelic(player)) return;

        if (cardPlay.Card?.Type == CardType.Attack && _dealtUnblockedDmg)
            await EmotionSystem.AddPositive(player, 1);
        _dealtUnblockedDmg = false;
    }

    private void FlushFullBlock()
    {
        if (_currentAttacker == null) return;
        if (_currentAttackFullyBlockedHits >= _currentAttackTotalHits
            && _currentAttackTotalHits > 0)
        {
            _fullyBlockedHitCount++;
            Log($"Full block vs {_currentAttacker.Name}: {_currentAttackFullyBlockedHits}/{_currentAttackTotalHits} hits blocked, total={_fullyBlockedHitCount}");
        }
        _currentAttacker = null;
        _currentAttackTotalHits = 0;
        _currentAttackFullyBlockedHits = 0;
    }

    // ═══════════════════ 回合结束：计入未格挡+完全格挡 ═══════════════════

    public override async Task BeforeSideTurnEnd(
        PlayerChoiceContext _, CombatSide side, IEnumerable<Creature> participants)
    {
        if (Owner == null) return;
        var player = Owner.Player;
        if (player == null || !EmotionSystem.HasQGRelic(player)) return;

        // 每段完全格挡 +1 正面（逐段计数）
        if (_fullyBlockedHitCount > 0)
        {
            _positiveCount += _fullyBlockedHitCount;
            Log($"Full block: +{_fullyBlockedHitCount} positive, total={_positiveCount}");
        }

        // 每段未格挡 +1 负面（逐段计数）
        Log($"SideTurnEnd side={side}: unblocked={_unblockedHitCount} fullyBlocked={_fullyBlockedHitCount} pos={_positiveCount} neg={_negativeCount}");
        _negativeCount += _unblockedHitCount;

        _unblockedHitCount = 0;
        _fullyBlockedHitCount = 0;
        FlushFullBlock();
    }

    // ═══════════════════ 死亡：立刻获得情感 ═══════════════════

    public override async Task AfterDeath(
        PlayerChoiceContext _, Creature creature, bool wasRemovalPrevented, float deathAnimLength)
    {
        if (Owner == null || wasRemovalPrevented || creature == Owner) return;
        var player = Owner.Player;
        if (player == null || !EmotionSystem.HasQGRelic(player)) return;

        if (creature.Side == Owner.Side)
            await EmotionSystem.AddNegative(player, creature.IsPlayer ? 5 : 2);
        else
            await EmotionSystem.AddPositive(player, creature.HasPower<MinionPower>() ? 2 : 5);
    }

    // ═══════════════════ 战斗重置 ═══════════════════

    public override async Task BeforeCombatStartLate()
    {
        Scripts.Entry.Logger.Info("QGEmotionAcquisition.BeforeCombatStartLate: resetting counters");
        _combatStarted = false;
        _positiveCount = 0;
        _negativeCount = 0;
        _unblockedHitCount = 0;
        _fullyBlockedHitCount = 0;
        _dealtUnblockedDmg = false;
        FlushFullBlock();
    }

    // ─── 确保附加 ───

    public static async Task EnsureApplied(Player player)
    {
        if (!EmotionSystem.HasQGRelic(player)) return;

        // 先移除旧的（跨战斗可能 hook 失效），再加新的
        var existing = player.Creature.Powers.OfType<QGEmotionAcquisition>().FirstOrDefault();
        if (existing != null)
            await PowerCmd.Remove(existing);

        await PowerCmd.Apply<QGEmotionAcquisition>(
            null!, player.Creature, 1, player.Creature, null);
    }
}
