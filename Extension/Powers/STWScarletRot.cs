using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Combat.HealthBars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace SpaceTimeWitch.Extension.Powers;

[RegisterPower]
public class STWScarletRot : ModPowerTemplate, IHealthBarForecastSource
{

    public override PowerType Type => PowerType.Debuff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side != Owner.Side) return;
        await TriggerOnce();
    }

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side != Owner.Side) return;
        await TriggerOnce();
    }

    private async Task TriggerOnce()
    {
        if (!Owner.IsAlive || Amount <= 0) return;

        await CreatureCmd.Damage(
            new ThrowingPlayerChoiceContext(), Owner,
            Amount,
            ValueProp.Unblockable | ValueProp.Unpowered,
            null, null);

        if (Owner.IsAlive)
            await PowerCmd.Decrement(this);
    }

    public IEnumerable<HealthBarForecastSegment> GetHealthBarForecastSegments(HealthBarForecastContext context)
    {
        var count = (int)Amount;
        if (count <= 0) yield break;

        // 回合开始：绿色（Amount）
        foreach (var s in HealthBarForecasts.Single(count,
            new Color(0.2f, 0.8f, 0.2f),
            HealthBarForecastGrowthDirection.FromLeft))
            yield return s;

        // 回合结束：紫色（Amount-1，因为回合开始已触发一次递减）
        if (count > 1)
        {
            foreach (var s in HealthBarForecasts.Single(count - 1,
                new Color(0.6f, 0.2f, 0.8f),
                HealthBarForecastGrowthDirection.FromLeft,
                order: 1))
                yield return s;
        }
    }

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"res://images/Extension/Powers/{GetType().Name}.png",
        BigIconPath: $"res://images/Extension/Powers/{GetType().Name}.png"
    );

}