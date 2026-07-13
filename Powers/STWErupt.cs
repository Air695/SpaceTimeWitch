using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;
using SpaceTimeWitch.Scripts;
using STS2RitsuLib.Combat.SecondaryResources;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace SpaceTimeWitch.Powers;

[RegisterPower]
public class STWErupt : ModPowerTemplate, ISecondaryResourceHookListener
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"res://images/SpaceTimeWitch/Powers/{GetType().Name}.png",
        BigIconPath: $"res://images/SpaceTimeWitch/Powers/{GetType().Name}.png"
    );

    public async Task AfterSecondaryResourceChanged(SecondaryResourceChangeContext context)
    {
        if (context.Definition.Id != ModChronoResources.Id) return;
        if (context.NewAmount >= context.OldAmount) return; // 不是消耗
        if (context.Player.Creature != Owner) return;

        var spent = context.OldAmount - context.NewAmount;
        var totalDamage = Amount * spent;

        await CreatureCmd.Damage(
            new BlockingPlayerChoiceContext(),
            CombatState.HittableEnemies,
            totalDamage,
            ValueProp.Unpowered,
            Owner,
            null
        );
    }
}
