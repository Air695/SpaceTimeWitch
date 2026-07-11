using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using SpaceTimeWitch.Character;
using SpaceTimeWitch.Commands;
using STS2RitsuLib.Interop.AutoRegistration;

namespace SpaceTimeWitch.Cards.Uncommon;

[RegisterCard(typeof(SpaceTimeWitchCardPool))]
public class ChronoFlow : SpaceTimeWitchCards
{
    protected override HashSet<CardTag> CanonicalTags =>
    [
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CardsVar(3),
        new CardsVar("StoreCount", 1),
        new CardsVar("RetrieveCount", 1)
    ];


    public ChronoFlow()
        : base(
            baseCost:1,
            type: CardType.Skill,
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

        var drawCount = DynamicVars.Cards.IntValue;
        await CardPileCmd.Draw(choiceContext, drawCount, owner);

        var storeMax = DynamicVars["StoreCount"].IntValue;
        var handCards = PileType.Hand.GetPile(owner).Cards.ToList();
        if (handCards.Count > 0)
        {
            var toStore = await CardSelectCmd.FromSimpleGrid(
                choiceContext, handCards, owner,
                new CardSelectorPrefs(SharedDepositPrompt, 0, storeMax));

            foreach (var card in toStore)
                await PersonalSpaceCmd.Store(owner, card);
        }

        var retrieveMax = DynamicVars["RetrieveCount"].IntValue;
        var psCards = PersonalSpaceCmd.GetCards(owner).ToList();
        if (psCards.Count > 0)
        {
            var toRetrieve = await CardSelectCmd.FromSimpleGrid(
                choiceContext, psCards, owner,
                new CardSelectorPrefs(SharedWithdrawPrompt, 0, retrieveMax));

            foreach (var card in toRetrieve)
                await PersonalSpaceCmd.Retrieve(owner, card);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Cards.UpgradeValueBy(1);
        DynamicVars["StoreCount"].UpgradeValueBy(1);
        DynamicVars["RetrieveCount"].UpgradeValueBy(1);
    }
}