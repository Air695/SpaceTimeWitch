using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using SpaceTimeWitch.Character;
using SpaceTimeWitch.Commands;
using STS2RitsuLib.Interop.AutoRegistration;

namespace SpaceTimeWitch.Cards.Rare;

[RegisterCard(typeof(SpaceTimeWitchCardPool))]
public class ChronoTruesight : SpaceTimeWitchCards
{
    protected override HashSet<CardTag> CanonicalTags =>
    [
        CardTags.Reproduce,
        CardTags.LJSK
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
    ];

    // ChronoMark cost set via SetChronoMarkCost(2) in constructor

    public ChronoTruesight()
        : base(
            baseCost:0,
            type: CardType.Skill,
            rarity: CardRarity.Rare,
            target: TargetType.Self
        )
    {
        SetChronoMarkCost(2);
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var chosen = await DiscoverCmd.Discover(
            choiceContext, Owner,
            cardType: null,
            offerCount: 5,
            minCount: 0,
            maxCount: 1,
            prompt: SharedChooseCardPrompt,
            extraFilter: c => c.Rarity == CardRarity.Rare,   // ֻѡϡ��
            sourceIsUpgraded: IsUpgraded
        );

        var card = chosen.FirstOrDefault();
        if (card == null) return;

        await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Hand, creator: card.Owner);
    }

    protected override void OnUpgrade()
    {
        SetChronoMarkCost(2);
    }
}