using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using SpaceTimeWitch.Character;
using SpaceTimeWitch.Commands;
using STS2RitsuLib.Interop.AutoRegistration;

namespace SpaceTimeWitch.Cards.Common;

[RegisterCard(typeof(SpaceTimeWitchCardPool))]
public class ChronoDispatch : SpaceTimeWitchCards
{
    protected override HashSet<CardTag> CanonicalTags =>
    [
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CardsVar(3)
    ];


    public ChronoDispatch()
        : base(
            baseCost:0,
            type: CardType.Skill,
            rarity: CardRarity.Common,
            target: TargetType.Self
        )
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Retain,CardKeyword.Exhaust];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var owner = Owner;
        if (owner?.Creature == null) return;

        var cards = PersonalSpaceCmd.GetCards(owner);
        if (cards.Count == 0) return;

        var maxSelect = DynamicVars.Cards.IntValue;

        var selected = await CardSelectCmd.FromSimpleGrid(
            choiceContext,
            cards.ToList(),
            owner,
            new CardSelectorPrefs(SharedWithdrawPrompt, 0, maxSelect)
        );

        foreach (var card in selected)
            await PersonalSpaceCmd.Retrieve(owner, card);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Cards.UpgradeValueBy(1);
    }
}