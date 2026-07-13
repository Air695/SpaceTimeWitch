using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using SpaceTimeWitch.Character;
using SpaceTimeWitch.Commands;
using STS2RitsuLib.Interop.AutoRegistration;

namespace SpaceTimeWitch.Cards.Uncommon;

[RegisterCard(typeof(SpaceTimeWitchCardPool))]
public class STWDeploy : SpaceTimeWitchCards
{
    protected override HashSet<CardTag> CanonicalTags =>
    [
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
    ];


    public STWDeploy()
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

        var psCards = PersonalSpaceCmd.GetCards(owner).ToList();
        if (psCards.Count == 0) return;

        List<CardModel> offered;
        if (CurrentUpgradeLevel > 0)
        {
            offered = psCards;
        }
        else
        {
            var rng = owner.RunState.Rng.CombatCardGeneration;
            offered = psCards.OrderBy(c => c.Id).OrderBy(_ => rng.NextInt()).Take(3).ToList();
        }

        var chosen = (await CardSelectCmd.FromSimpleGrid(
                choiceContext, offered, owner,
                new CardSelectorPrefs(SharedChooseCardPrompt, 0, 1)))
            .FirstOrDefault();

        if (chosen == null) return;

        await CardPileCmd.Add(chosen, PileType.Draw);

        await CardCmd.AutoPlay(choiceContext, chosen, null);
    }

    protected override void OnUpgrade()
    {
    }
}