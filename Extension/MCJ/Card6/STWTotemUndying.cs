using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using SpaceTimeWitch.Character;
using SpaceTimeWitch.Scripts;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace SpaceTimeWitch.Extension.MCJ.Card6;

[RegisterRelic(typeof(SpaceTimeWitchRelicPool))]
public class STWTotemUndying : SpaceTimeWitchRelics
{
    public STWTotemUndying()
        : base(RelicRarity.Event)
    {
    }
    private bool _wasUsed;
    public override bool IsUsedUp => _wasUsed;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new HealVar(50m)
    ];

    public override RelicAssetProfile AssetProfile => new(
        IconPath: $"res://images/Extension/Relics/{GetType().Name}.png",
        IconOutlinePath: $"res://images/Extension/Relics/{GetType().Name}.png",
        BigIconPath: $"res://images/Extension/Relics/{GetType().Name}.png"
    );
    
    public bool WasUsed
    {
        get
        {
            return _wasUsed;
        }
        set
        {
            AssertMutable();
            _wasUsed = value;
            if (IsUsedUp)
            {
                Status = RelicStatus.Disabled;
            }
        }
    }

    public override bool ShouldDieLate(Creature creature)
    {
        if (creature != Owner.Creature)
        {
            return true;
        }
        if (WasUsed)
        {
            return true;
        }
        return false;
    }

    public override async Task AfterPreventingDeath(Creature creature)
    {
        Flash();
        WasUsed = true;
        var ctx = new BlockingPlayerChoiceContext();
        decimal amount = Math.Max(1m, creature.MaxHp * (DynamicVars.Heal.BaseValue / 100m));
        await CreatureCmd.Heal(creature, amount);
        await PowerCmd.Apply<PlatingPower>(ctx, creature, 10m, creature, null);
        await PowerCmd.Apply<RegenPower>(ctx, creature, 5m, creature, null);
    }

}