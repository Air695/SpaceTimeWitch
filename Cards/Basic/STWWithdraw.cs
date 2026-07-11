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
public class STWWithdraw : SpaceTimeWitchCards
{
    protected override HashSet<CardTag> CanonicalTags =>
    [
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CardsVar(1)
    ];

    public STWWithdraw()
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
        {
            if (CurrentUpgradeLevel > 0)
            {
                CardCmd.Upgrade(card);
            }
            await PersonalSpaceCmd.Retrieve(owner, card);
        }
        
    }

    protected override void OnUpgrade()
    {
    }
}