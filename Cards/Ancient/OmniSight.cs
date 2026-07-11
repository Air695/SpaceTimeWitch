using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using SpaceTimeWitch.Character;
using SpaceTimeWitch.Commands;
using SpaceTimeWitch.Extension.QG;
using STS2RitsuLib.Interop.AutoRegistration;

namespace SpaceTimeWitch.Cards.Ancient;

[RegisterCard(typeof(SpaceTimeWitchCardPool))]
public class OmniSight : SpaceTimeWitchCards
{
    protected override HashSet<CardTag> CanonicalTags => [CardTags.Reproduce];
    // ChronoMark cost set via SetChronoMarkCost(5) in constructor
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust,CardKeyword.Retain];

    public OmniSight()
        : base(baseCost: 0, type: CardType.Skill, rarity: CardRarity.Ancient, target: TargetType.Self)
    {
        SetChronoMarkCost(3);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var owner = Owner;
        if (owner?.Creature == null) return;

        var pools = CardPoolSelectCmd.GetAllowedPools();
        if (pools.Count == 0) return;

        var pool = await CardPoolSelectCmd.SelectPool(
            choiceContext, owner, pools, SharedChoosePoolPrompt);
        if (pool == null) return;

        await ShowAllCardsFromPool(choiceContext, owner, pool);
    }

    private async Task ShowAllCardsFromPool(PlayerChoiceContext context, Player owner, CardPoolModel pool)
    {
        var allCards = pool
            .GetUnlockedCards(owner.UnlockState, owner.RunState.CardMultiplayerConstraint)
            .Select(c =>
            {
                var card = owner.Creature.CombatState.CreateCard(c, owner);
                if (IsUpgraded)
                {
                    card.UpgradePreviewType = CardUpgradePreviewType.Combat;
                    card.UpgradeInternal();
                }
                return card;
            })
            .ToList();

        var result = await CardSelectCmd.FromSimpleGrid(
            context, allCards, owner,
            new CardSelectorPrefs(SharedChooseCardPrompt, 0, 1)
        );
        if (result.FirstOrDefault() is not { } chosen)
            return;

        var finalCard = owner.Creature.CombatState.CreateCard(chosen.CanonicalInstance, owner);
        if (IsUpgraded)
            finalCard.UpgradeInternal();

        await CardPileCmd.AddGeneratedCardToCombat(finalCard, PileType.Hand, creator: finalCard.Owner);
    }

    protected override void OnUpgrade()
    {
    }
}
