using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using SpaceTimeWitch.Character;
using SpaceTimeWitch.Powers;
using STS2RitsuLib.Interop.AutoRegistration;

namespace SpaceTimeWitch.Cards.Uncommon;

[RegisterCard(typeof(SpaceTimeWitchCardPool))]
public class ChronoEvolution : SpaceTimeWitchCards
{
    protected override HashSet<CardTag> CanonicalTags =>
    [
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<STWEvolution>(3m)
    ];


    public ChronoEvolution()
        : base(
            baseCost:1,
            type: CardType.Power,
            rarity: CardRarity.Uncommon,
            target: TargetType.Self
        )
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var owner = Owner;
        if (owner?.Creature == null) return;

        var amount = DynamicVars["STWEvolution"].IntValue;
        await PowerCmd.Apply<STWEvolution>(choiceContext, owner.Creature, amount, owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["STWEvolution"].UpgradeValueBy(1m);
    }
}