using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Cards;
using SpaceTimeWitch.Character;
using STS2RitsuLib.Interop.AutoRegistration;

namespace SpaceTimeWitch.Cards.Rare;

[RegisterCard(typeof(SpaceTimeWitchCardPool))]
public class STWAdvance : SpaceTimeWitchCards
{
    protected override HashSet<CardTag> CanonicalTags =>
    [
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CardsVar(30)
    ];

    public STWAdvance()
        : base(
            baseCost:0,
            type: CardType.Skill,
            rarity: CardRarity.Rare,
            target: TargetType.Self
        )
    {
        SetChronoMarkCost(1);
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
    
    protected override IEnumerable<IHoverTip> CardSpecificHoverTips =>
    [
        HoverTipFactory.FromCard<Debt>(),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var owner = Owner;
        if (owner?.Creature == null) return;

        await PlayerCmd.GainGold(DynamicVars.Cards.IntValue, owner);

        if (owner.RunState.Rng.CombatCardGeneration.NextDouble() < 0.05)
        {
            var debt = owner.Creature.CombatState.CreateCard<Debt>(owner);
            var deckCard = owner.RunState.CloneCard(debt);
            CombatState.RemoveCard(debt);
            await CardPileCmd.Add(deckCard, PileType.Deck, clonedBy: this);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Cards.UpgradeValueBy(5);
    }
}