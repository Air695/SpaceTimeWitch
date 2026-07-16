using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using SpaceTimeWitch.Character;
using STS2RitsuLib.Interop.AutoRegistration;

namespace SpaceTimeWitch.Cards.Common;

[RegisterCard(typeof(SpaceTimeWitchCardPool))]
public class STWArmaments : SpaceTimeWitchCards
{
    protected override HashSet<CardTag> CanonicalTags =>
    [
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CardsVar(2)
    ];

    public STWArmaments()
        : base(
            baseCost:0,
            type: CardType.Skill,
            rarity: CardRarity.Common,
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

        var count = DynamicVars.Cards.IntValue;

        var selected = await CardSelectCmd.FromHand(
            prefs: new CardSelectorPrefs(SharedChooseCardPrompt, 0, count),
            context: choiceContext,
            player: owner,
            filter: c => c.IsUpgradable,
            source: this);

        foreach (var card in selected)
        {
            CardCmd.Upgrade(card);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Cards.UpgradeValueBy(1);
    }
}