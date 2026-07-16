using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using SpaceTimeWitch.Character;
using SpaceTimeWitch.Commands;
using SpaceTimeWitch.Scripts;
using STS2RitsuLib.Combat.SecondaryResources;
using STS2RitsuLib.Interop.AutoRegistration;

namespace SpaceTimeWitch.Cards.Rare;

[RegisterCard(typeof(SpaceTimeWitchCardPool))]
public class ChronoRecall : SpaceTimeWitchCards
{
    private static readonly LocString RecallPrompt = new("cards", "SPACE_TIME_WITCH_CARD_CHRONO_RECALL.selectionScreenPrompt");

    protected override HashSet<CardTag> CanonicalTags =>
    [
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CardsVar(1)
    ];

    public ChronoRecall()
        : base(
            baseCost: 0,
            type: CardType.Skill,
            rarity: CardRarity.Rare,
            target: TargetType.Self
        )
    {
        SetChronoMarkCost(1);
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var psCards = PersonalSpaceCmd.GetCards(Owner);

        var allCards = PileType.Draw.GetPile(Owner).Cards
            .Concat(PileType.Discard.GetPile(Owner).Cards)
            .Concat(PileType.Exhaust.GetPile(Owner).Cards)
            .Concat(psCards)
            .ToList();

        if (allCards.Count == 0)
            return;

        int maxSelect = DynamicVars.Cards.IntValue;

        var selected = await CardSelectCmd.FromSimpleGrid(
            choiceContext,
            allCards,
            Owner,
            new CardSelectorPrefs(RecallPrompt, 0, maxSelect)
        );

        if (selected.Any())
        {
            var fromPs = selected.Where(c => psCards.Contains(c)).ToList();
            foreach (var card in fromPs)
                await PersonalSpaceCmd.Retrieve(Owner, card);

            var fromOther = selected.Except(fromPs).ToList();
            if (fromOther.Any())
                await CardPileCmd.Add(fromOther, PileType.Hand);
        }
    }

    protected override void OnUpgrade()
    {
        SetChronoMarkCost(2);
        RemoveKeyword(CardKeyword.Exhaust);
    }
}