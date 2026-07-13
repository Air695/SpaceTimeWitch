using System.Linq;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using SpaceTimeWitch.Character;
using SpaceTimeWitch.Extension.QG;
using STS2RitsuLib.Interop.AutoRegistration;

namespace SpaceTimeWitch.Cards.Rare;

[RegisterCard(typeof(SpaceTimeWitchCardPool))]
public class STWGenesis : SpaceTimeWitchCards
{
    protected override HashSet<CardTag> CanonicalTags =>
    [
        CardTags.Reproduce,
        CardTags.FSK
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CardsVar(10)
    ];

    public STWGenesis()
        : base(
            baseCost:0,
            type: CardType.Skill,
            rarity: CardRarity.Rare,
            target: TargetType.Self
        )
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var owner = Owner;
        if (owner?.Creature == null) return;

        var excludedPoolIds = new HashSet<ModelId>(
            ModelDb.CardPool<STWLRA>().AllCards.Concat(
            ModelDb.CardPool<STWYC>().AllCards).Select(c => c.Id));

        var allCards = ModelDb.AllCards
            .Where(c => c.CanBeGeneratedInCombat && !excludedPoolIds.Contains(c.Id))
            .GroupBy(c => c.Id)
            .Select(g => g.First())
            .OrderBy(c => c.Id)
            .ToList();

        if (allCards.Count == 0) return;

        var rng = owner.RunState.Rng.CombatCardGeneration;

        var amount = DynamicVars.Cards.IntValue;

        var choices = allCards
            .OrderBy(_ => rng.NextInt())
            .Take(amount)
            .Select(c => owner.Creature.CombatState.CreateCard(c, owner))
            .ToList();

        if (IsUpgraded)
        {
            foreach (var card in choices)
                CardCmd.Upgrade(card);
        }

        var chosen = (await CardSelectCmd.FromSimpleGrid(
                choiceContext, choices, owner,
                new CardSelectorPrefs(
                    new LocString("cards", "STW_DISCOVER_PROMPT"),
                    minCount: 0, maxCount: 1)))
            .FirstOrDefault();

        if (chosen == null) return;

        await CardPileCmd.AddGeneratedCardToCombat(chosen, PileType.Hand, creator: chosen.Owner);
    }

    protected override void OnUpgrade()
    {
    }
}