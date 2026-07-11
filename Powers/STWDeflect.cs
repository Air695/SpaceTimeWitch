using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using SpaceTimeWitch.Commands;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace SpaceTimeWitch.Powers;

[RegisterPower]
public class STWDeflect : ModPowerTemplate
{

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"res://images/SpaceTimeWitch/Powers/{GetType().Name}.png",
        BigIconPath: $"res://images/SpaceTimeWitch/Powers/{GetType().Name}.png"
    );
    
    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player.Creature != Owner) return;
        
        await PowerCmd.Remove(this);

        var wasAttackedLastTurn = CombatManager.Instance.History.Entries
            .OfType<DamageReceivedEntry>()
            .Any(e => e.Receiver == Owner
                      && e.Dealer != null
                      && e.Dealer.Side == CombatSide.Enemy);

        if (wasAttackedLastTurn)
        {
            Flash();
            await ChronoMark.Gain(Owner, Amount);
        }
    }

}