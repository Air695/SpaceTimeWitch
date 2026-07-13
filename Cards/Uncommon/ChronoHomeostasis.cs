using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using SpaceTimeWitch.Character;
using STS2RitsuLib.Interop.AutoRegistration;

namespace SpaceTimeWitch.Cards.Uncommon;

[RegisterCard(typeof(SpaceTimeWitchCardPool))]
public class ChronoHomeostasis : SpaceTimeWitchCards
{
    protected override HashSet<CardTag> CanonicalTags =>
    [
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new HealVar(4)
    ];

    public ChronoHomeostasis()
        : base(
            baseCost:0,
            type: CardType.Skill,
            rarity: CardRarity.Uncommon,
            target: TargetType.Self
        )
    {
        SetChronoMarkCost(1);
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var owner = Owner;
        if (owner?.Creature == null) return;
        
        await CreatureCmd.Heal(owner.Creature,DynamicVars.Heal.IntValue);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Heal.UpgradeValueBy(2);
    }
}