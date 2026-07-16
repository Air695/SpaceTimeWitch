using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Rooms;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Combat.HealthBars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace SpaceTimeWitch.Extension.Powers;

[RegisterPower]
public class STWDestinedDeath : ModPowerTemplate, IHealthBarForecastSource
{

    public override PowerType Type => PowerType.Debuff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterSideTurnStart(
        CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side != Owner.Side) return;
        if (!Owner.IsAlive || Amount <= 0) return;

        var pct = GetPercentage();
        var reduction = (int)(Owner.MaxHp * pct * Amount);
        if (reduction <= 0) return;

        await CreatureCmd.SetCurrentHp(Owner, Math.Max(0, Owner.CurrentHp - reduction));
        if (Owner.IsDead) return;
        await CreatureCmd.SetMaxHp(Owner, Math.Max(1, Owner.MaxHp - reduction));
    }

    private decimal GetPercentage()
    {
        var isMulti = Owner.CombatState.Players.Count() > 1;
        var isBoss = Owner.CombatState.Encounter?.RoomType == RoomType.Boss;

        if (isMulti && isBoss) return 0.015m;
        if (isMulti) return 0.03m;
        if (isBoss) return 0.03m;
        return 0.06m;
    }

    private decimal GetPercentageDisplay()
    {
        if (Owner?.CombatState == null) return 4m;
        var isMulti = Owner.CombatState.Players.Count() > 1;
        var isBoss = Owner.CombatState.Encounter?.RoomType == RoomType.Boss;
        if (isMulti && isBoss) return 1.5m;
        if (isMulti) return 3m;
        if (isBoss) return 3m;
        return 6m;
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new ComputedDynamicVar("Pct", 4m, _ => GetPercentageDisplay())
    ];

    public IEnumerable<HealthBarForecastSegment> GetHealthBarForecastSegments(HealthBarForecastContext context)
    {
        var amount = (int)Amount;
        if (amount <= 0) return [];

        var pct = GetPercentage();
        var reduction = (int)(context.Creature.MaxHp * pct * amount);
        if (reduction <= 0) return [];

        return HealthBarForecasts.Single(reduction,
            new Color(0.1f, 0.1f, 0.1f),
            HealthBarForecastGrowthDirection.FromLeft);
    }

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"res://images/Extension/Powers/{GetType().Name}.png",
        BigIconPath: $"res://images/Extension/Powers/{GetType().Name}.png"
    );

}