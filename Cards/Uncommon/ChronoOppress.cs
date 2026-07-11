using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using SpaceTimeWitch.Character;
using STS2RitsuLib.Interop.AutoRegistration;

namespace SpaceTimeWitch.Cards.Uncommon;

[RegisterCard(typeof(SpaceTimeWitchCardPool))]
public class ChronoOppress : SpaceTimeWitchCards
{
    protected override HashSet<CardTag> CanonicalTags =>
    [
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<WeakPower>(2m),
        new PowerVar<StrengthPower>(2m)
    ];

    public ChronoOppress()
        : base(
            baseCost: 1,
            type: CardType.Skill,
            rarity: CardRarity.Uncommon,
            target: TargetType.AnyEnemy
        )
    {
        SetChronoMarkCost(1);
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var owner = Owner;
        if (owner?.Creature == null) return;

        var wamount = DynamicVars["WeakPower"].IntValue;
        await PowerCmd.Apply<WeakPower>(choiceContext, play.Target, wamount,owner.Creature,this);
        var samount = -DynamicVars["StrengthPower"].IntValue;
        await PowerCmd.Apply<StrengthPower>(choiceContext, play.Target, samount,owner.Creature,this);

    }

    protected override void OnUpgrade()
    {
        DynamicVars["WeakPower"].UpgradeValueBy(1);
        DynamicVars["StrengthPower"].UpgradeValueBy(1);
    }
}