using System.Linq;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using SpaceTimeWitch.Character;
using SpaceTimeWitch.Extension.QG;
using STS2RitsuLib.Interop.AutoRegistration;

namespace SpaceTimeWitch.Cards.Common;

[RegisterCard(typeof(SpaceTimeWitchCardPool))]
public class STWFate : SpaceTimeWitchCards
{
    protected override HashSet<CardTag> CanonicalTags =>
    [
        CardTags.FSK
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
    ];


    public STWFate()
        : base(
            baseCost:0,
            type: CardType.Skill,
            rarity: CardRarity.Common,
            target: TargetType.Self
        )
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [];
    
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

        var excludedPoolIds = new HashSet<ModelId>(
            ModelDb.CardPool<STWLRA>().AllCards.Concat(
            ModelDb.CardPool<STWYC>().AllCards).Select(c => c.Id));

        var allCards = ModelDb.AllCards
            .Where(c => c.CanBeGeneratedInCombat && !excludedPoolIds.Contains(c.Id))
            .ToList();

        if (allCards.Count == 0) return;

        var picked = owner.RunState.Rng.CombatCardGeneration.NextItem(allCards);
        var card = owner.Creature.CombatState.CreateCard(picked, owner);

        if (CurrentUpgradeLevel > 0)
            CardCmd.Upgrade(card);

        await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Hand, creator: card.Owner);
    }

    protected override void OnUpgrade()
    {
    }
}