using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Scaffolding.Content;

namespace SpaceTimeWitch.Field;

/// <summary>
/// 场地能力基类。应用在玩家 Creature 上，同一时间只有一个场地。
/// 内置回合级别的去重：无论挂在几个 Creature 上，效果每回合只触发一次。
/// 额外回合（extra turn）不触发。
/// 子类覆写 OnFieldSideTurnStart 实现全局效果。
/// </summary>
public abstract class FieldPowerBase : ModPowerTemplate
{
    public override PowerType Type => PowerType.None;
    public override PowerStackType StackType => PowerStackType.Single;

    /// <summary>场地背景行为类型</summary>
    public abstract FieldBackgroundType BackgroundType { get; }

    /// <summary>背景资源路径</summary>
    public abstract string BackgroundPath { get; }

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"res://images/SpaceTimeWitch/Powers/{GetType().Name}.png"
    );

    /// <summary>
    /// 获取战斗中所有生物（玩家侧 + 敌人侧）。
    /// </summary>
    protected IEnumerable<Creature> AllCreatures =>
        CombatState.Allies.Concat(CombatState.Enemies);

    // ── 去重机制 ──
    // BeforeSideTurnStart 会在每个拥有此能力的 Creature 上各触发一次。
    // 用静态标记确保实际效果每回合只执行一次。
    // 标记在敌方回合开始时重置，为下一轮玩家回合做好准备。

    private static bool _hasProcessedThisTurn;

    /// <summary>每场战斗开始时重置去重标记。</summary>
    public override Task BeforeCombatStart() { _hasProcessedThisTurn = false; return Task.CompletedTask; }

    public sealed override async Task BeforeSideTurnStart(
        PlayerChoiceContext choiceContext, CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side != CombatSide.Player) return;

        // 额外回合不触发
        if (CombatManager.Instance.PlayersTakingExtraTurn.Count > 0) return;

        // 去重：同一回合只触发一次
        if (_hasProcessedThisTurn) return;
        _hasProcessedThisTurn = true;

        await OnFieldSideTurnStart(choiceContext, side, combatState);
    }

    /// <summary>
    /// 每回合开始时触发一次（已去重，跳过额外回合）。子类覆写此方法实现场地效果。
    /// </summary>
    protected virtual Task OnFieldSideTurnStart(
        PlayerChoiceContext choiceContext, CombatSide side, ICombatState combatState)
        => Task.CompletedTask;

    // 敌方回合开始时重置去重标记，为下一轮玩家回合做准备
    public override Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side == CombatSide.Enemy)
            _hasProcessedThisTurn = false;
        return Task.CompletedTask;
    }
}
