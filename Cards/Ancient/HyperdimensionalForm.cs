using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using SpaceTimeWitch.Character;
using SpaceTimeWitch.Powers;
using STS2RitsuLib.Interop.AutoRegistration;

namespace SpaceTimeWitch.Cards.Rare;

[RegisterCard(typeof(SpaceTimeWitchCardPool))]
public class HyperdimensionalForm : SpaceTimeWitchCards
{
    protected override HashSet<CardTag> CanonicalTags =>
    [
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<Hyperdimensional>(1m)
    ];

    public HyperdimensionalForm()
        : base(
            baseCost: 3,
            type: CardType.Power,
            rarity: CardRarity.Ancient,
            target: TargetType.Self
        )
    {
        SetChronoMarkCost(3);
    }
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Ethereal];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var creature = Owner.Creature;
        var target = play.Target ?? creature;
        
        var hAmount = DynamicVars["Hyperdimensional"].IntValue;
        await PowerCmd.Apply<Hyperdimensional>(choiceContext, target, hAmount, creature, this);
    }

    protected override void OnUpgrade()
    {
        RemoveKeyword(CardKeyword.Ethereal);
    }
}