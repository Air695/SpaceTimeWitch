using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using SpaceTimeWitch.Character;
using SpaceTimeWitch.Commands;
using SpaceTimeWitch.Powers;
using SpaceTimeWitch.Scripts;
using STS2RitsuLib.Combat.SecondaryResources;
using STS2RitsuLib.Interop.AutoRegistration;

namespace SpaceTimeWitch.Cards.Ancient;

[RegisterCard(typeof(SpaceTimeWitchCardPool))]
public class GravenEternityDawn : SpaceTimeWitchCards
{
    protected override HashSet<CardTag> CanonicalTags =>
    [
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        SecondaryResourceVars.For("ChronoMark", ModChronoResources.Id, 3),
        new PowerVar<GravenEternityDawnPower>(1m),
    ];

    public GravenEternityDawn()
        : base(
            baseCost: 1,
            type: CardType.Power,
            rarity: CardRarity.Ancient,
            target: TargetType.Self
        )
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Innate];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var owner = Owner;
        if (owner?.Creature == null) return;
        
        var markAmount = DynamicVars["ChronoMark"].IntValue;
        await ChronoMark.Gain(owner.Creature, markAmount);

        var creature = Owner.Creature;
        
        var gAmount = DynamicVars["GravenEternityDawnPower"].IntValue;
        await PowerCmd.Apply<GravenEternityDawnPower>(choiceContext, creature, gAmount, creature, this);
        
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}