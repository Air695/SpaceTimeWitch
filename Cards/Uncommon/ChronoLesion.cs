using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using SpaceTimeWitch.Character;
using SpaceTimeWitch.Commands;
using SpaceTimeWitch.Powers;
using STS2RitsuLib.Interop.AutoRegistration;

namespace SpaceTimeWitch.Cards.Uncommon;

[RegisterCard(typeof(SpaceTimeWitchCardPool))]
public class ChronoLesion : SpaceTimeWitchCards
{
    protected override HashSet<CardTag> CanonicalTags =>
    [
        CardTags.MarkA
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new IntVar("ChronoMark", 1m),
        new PowerVar<STWLesion>(1m)
    ];


    public ChronoLesion()
        : base(
            baseCost: 1,
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
        
        var samount = DynamicVars["STWLesion"].IntValue;
        await PowerCmd.Apply<STWLesion>(choiceContext, owner.Creature, samount,owner.Creature,this);
        
        var amount = DynamicVars["ChronoMark"].IntValue;
        if (CurrentUpgradeLevel > 0)
            await ChronoMark.Gain(owner.Creature, amount);

    }

    protected override void OnUpgrade()
    {

    }
}