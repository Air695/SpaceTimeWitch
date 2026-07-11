using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using SpaceTimeWitch.Character;
using SpaceTimeWitch.Commands;
using STS2RitsuLib.Interop.AutoRegistration;

namespace SpaceTimeWitch.Cards.Basic;

[RegisterCard(typeof(SpaceTimeWitchCardPool))]
[RegisterCharacterStarterCard(typeof(SpaceTimeWitch.Character.SpaceTimeWitch))]
public class ChronoRiftMirage : SpaceTimeWitchCards
{
    protected override HashSet<CardTag> CanonicalTags =>
    [
        CardTags.Reproduce,
        CardTags.LJSK
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CardsVar(5)
    ];

    // ChronoMark cost set via SetChronoMarkCost(1) in constructor

    public ChronoRiftMirage()
        : base(
            baseCost: 0,
            type: CardType.Skill,
            rarity: CardRarity.Basic,
            target: TargetType.Self
        )
    {
        SetChronoMarkCost(1);
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var chosen = await DiscoverCmd.Discover(
            choiceContext, Owner,
            cardType: CardType.Skill,
            offerCount: DynamicVars.Cards.IntValue,
            minCount: 0,
            maxCount: 1,
            prompt: new LocString("cards", "STW_DISCOVER_PROMPT_S"),
            extraFilter: c =>c.Rarity<=CardRarity.Uncommon,
            sourceIsUpgraded: IsUpgraded
        );

        var card = chosen.FirstOrDefault();
        if (card == null) return;

        await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Hand, creator: card.Owner);
    }

    protected override void OnUpgrade()
    {
        SetChronoMarkCost(1);
    }
}