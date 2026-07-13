using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using SpaceTimeWitch.Character;
using SpaceTimeWitch.Commands;
using SpaceTimeWitch.Relics;
using STS2RitsuLib.Interop.AutoRegistration;

namespace SpaceTimeWitch.Cards.Uncommon;

[RegisterCard(typeof(SpaceTimeWitchCardPool))]
public class ChronoConjure : SpaceTimeWitchCards
{
    protected override HashSet<CardTag> CanonicalTags =>
    [
        CardTags.Reproduce,
        CardTags.LJSK
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CardsVar(3)
    ];

    public ChronoConjure()
        : base(
            baseCost:0,
            type: CardType.Skill,
            rarity: CardRarity.Uncommon,
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

        var activeTags = owner.Relics
            .OfType<ITagRelic>()
            .Select(r => r.AssociatedTag)
            .ToHashSet();

        if (activeTags.Count == 0) return;

        var candidates = ModelDb.AllCardPools
            .SelectMany(p => p.GetUnlockedCards(
                owner.UnlockState,
                owner.RunState.CardMultiplayerConstraint))
            .Where(c => c.CanBeGeneratedInCombat
                        && c.Tags.Any(t => activeTags.Contains(t)))
            .GroupBy(c => c.Id)
            .Select(g => g.First())
            .OrderBy(c => c.Id)
            .ToList();

        if (candidates.Count == 0) return;

        int count = DynamicVars.Cards.IntValue;
        var rng = owner.RunState.Rng.CombatCardGeneration;
        var combatState = owner.Creature.CombatState;

        for (int i = 0; i < count && candidates.Count > 0; i++)
        {
            var picked = rng.NextItem(candidates);
            candidates.Remove(picked);

            var card = combatState.CreateCard(picked, owner);
            await PersonalSpaceCmd.Store(owner, card);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Cards.UpgradeValueBy(2);
    }
}