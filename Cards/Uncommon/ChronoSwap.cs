using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using SpaceTimeWitch.Commands;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Combat.SecondaryResources;
using SpaceTimeWitch.Scripts;
using SpaceTimeWitch.Character;

namespace SpaceTimeWitch.Cards.Uncommon;

[RegisterCard(typeof(SpaceTimeWitchCardPool))]
public class ChronoSwap : SpaceTimeWitchCards
{
    protected override HashSet<CardTag> CanonicalTags =>
    [
        CardTags.Reproduce
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CardsVar(SpaceTimeWitchSettings.DefaultDiscoverOfferCount)
    ];


    public ChronoSwap()
        : base(
            baseCost: 0,
            type: CardType.Skill,
            rarity: CardRarity.Uncommon,
            target: TargetType.Self
        )
    {
        SetChronoMarkCost(1);
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var owner = Owner;
        if (owner?.Creature == null) return;
        
        var handCards = PileType.Hand.GetPile(owner).Cards.ToList();
        if (handCards.Count == 0) return;

        var chosen = (await CardSelectCmd.FromSimpleGrid(
                choiceContext, handCards, owner,
                new CardSelectorPrefs(SharedChooseCardPrompt, 1, 1)))
            .FirstOrDefault();
        if (chosen == null) return;

        await CardCmd.Exhaust(choiceContext, chosen);

        var candidates = chosen.Pool.AllCards
            .Where(c => c.Id != chosen.Id && c.CanBeGeneratedInCombat);

        if (!candidates.Any()) return;

        var (cw, uw, rw) = WeightedCardSelectCmd.GetConfiguredWeights();
        var result = await WeightedCardSelectCmd.PickFromCards(
            choiceContext, owner, candidates,
            offerCount: SpaceTimeWitchSettings.DiscoverOfferCount,
            commonWeight: cw, uncommonWeight: uw, rareWeight: rw,
            prompt: SharedChooseCardPrompt,
            upgradeOffered: CurrentUpgradeLevel > 0);

        if (result == null) return;
        if (CurrentUpgradeLevel > 0)
            CardCmd.Upgrade(result);

        await CardPileCmd.AddGeneratedCardToCombat(result, PileType.Hand, creator: result.Owner);
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Retain);
        this.SecondaryCosts().Clear(ModChronoResources.Id);
    }
}