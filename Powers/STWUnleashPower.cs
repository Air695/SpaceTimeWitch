using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using SpaceTimeWitch.Commands;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace SpaceTimeWitch.Powers;

[RegisterPower]
public class STWUnleashPower : ModPowerTemplate
{

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"res://images/SpaceTimeWitch/Powers/{GetType().Name}.png",
        BigIconPath: $"res://images/SpaceTimeWitch/Powers/{GetType().Name}.png"
    );
    
    public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
    {
        if (cardPlay.Card.Type == CardType.Attack
            && cardPlay.Card.Owner.Creature == Owner)
        {
            await ChronoMark.Gain(Owner, Amount, cardPlay.Card);
        }
    }

}