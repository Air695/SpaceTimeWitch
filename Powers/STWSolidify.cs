using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using SpaceTimeWitch.Scripts;
using STS2RitsuLib.Combat.SecondaryResources;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace SpaceTimeWitch.Powers;

[RegisterPower]
public class STWSolidify : ModPowerTemplate, ISecondaryResourceHookListener
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
        if (context.NewAmount <= context.OldAmount) return; // 不是获得
        if (context.Player.Creature != Owner) return;

        Flash();
        var gained = context.NewAmount - context.OldAmount;
        var totalBlock = Amount * gained;
        await CreatureCmd.GainBlock(Owner, totalBlock, ValueProp.Unpowered, null);
    }
}
