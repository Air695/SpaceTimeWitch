using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using SpaceTimeWitch.Character;
using SpaceTimeWitch.Commands;
using STS2RitsuLib.Interop.AutoRegistration;

namespace SpaceTimeWitch.Cards.Basic;

[RegisterCard(typeof(SpaceTimeWitchCardPool))]
[RegisterCharacterStarterCard(typeof(SpaceTimeWitch.Character.SpaceTimeWitch))]
public class STWDeposit : SpaceTimeWitchCards
{
    protected override HashSet<CardTag> CanonicalTags =>
    [
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CardsVar(2)
    ];

    public STWDeposit()
        : base(
            baseCost: 0,
            type: CardType.Skill,
            rarity: CardRarity.Basic,
            target: TargetType.Self
        )
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Retain];
    
    public override async Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
    {
        if (card != this) return;
        var owner = Owner;
        if (owner?.Creature == null) return;
        await CardPileCmd.Draw(choiceContext, 1, owner);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var owner = Owner;
        if (owner?.Creature == null) return;

        var maxSelect = DynamicVars.Cards.IntValue;

        var selected = await CardSelectCmd.FromHand(
            prefs: new CardSelectorPrefs(SharedDepositPrompt, 0, maxSelect),
            context: choiceContext,
            player: owner,
            filter: null,
            source: this
        );

        foreach (var card in selected)
        {
            await PersonalSpaceCmd.Store(owner, card);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Cards.UpgradeValueBy(1);
    }
}