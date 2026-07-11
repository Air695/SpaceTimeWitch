using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using SpaceTimeWitch.Character;
using SpaceTimeWitch.Commands;
using STS2RitsuLib.Interop.AutoRegistration;

namespace SpaceTimeWitch.Cards.Token;

[RegisterCard(typeof(SpaceTimeWitchCardPool))]
public class InstantRift : SpaceTimeWitchCards
{
    protected override HashSet<CardTag> CanonicalTags =>
    [
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
    ];


    public InstantRift()
        : base(
            baseCost:0,
            type: CardType.Skill,
            rarity: CardRarity.Token,
            target: TargetType.Self
        )
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust,CardKeyword.Ethereal];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var owner = Owner;
        if (owner?.Creature == null) return;

        var handCards = PileType.Hand.GetPile(owner).Cards.ToList();
        var psCards = PersonalSpaceCmd.GetCards(owner).ToList();

        if (handCards.Count > 0)
        {
            var toDeposit = (await CardSelectCmd.FromSimpleGrid(
                    choiceContext, handCards, owner,
                    new CardSelectorPrefs(SharedDepositPrompt, 0, 1)))
                .FirstOrDefault();
            if (toDeposit != null)
            {
                await PersonalSpaceCmd.Store(owner, toDeposit);
                return;
            }
        }

        if (psCards.Count > 0)
        {
            var toWithdraw = (await CardSelectCmd.FromSimpleGrid(
                    choiceContext, psCards, owner,
                    new CardSelectorPrefs(SharedWithdrawPrompt, 0, 1)))
                .FirstOrDefault();
            if (toWithdraw != null)
                await PersonalSpaceCmd.Retrieve(owner, toWithdraw);
        }
    }

    protected override void OnUpgrade()
    {
        RemoveKeyword(CardKeyword.Ethereal);
        AddKeyword(CardKeyword.Retain);
    }
}