using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using SpaceTimeWitch.Character;
using SpaceTimeWitch.Commands;
using STS2RitsuLib.Interop.AutoRegistration;

namespace SpaceTimeWitch.Cards.Uncommon;

[RegisterCard(typeof(SpaceTimeWitchCardPool))]
public class PrismEngrave : SpaceTimeWitchCards
{
    protected override HashSet<CardTag> CanonicalTags =>
    [
        CardTags.Reproduce
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
    ];


    public PrismEngrave()
        : base(
            baseCost: 0,
            type: CardType.Skill,
            rarity: CardRarity.Uncommon,
            target: TargetType.Self
        )
    {
        SetChronoMarkCost(1);
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var owner = Owner;
        if (owner?.Creature == null) return;

        var pool = await CardPoolSelectCmd.SelectPool(
            choiceContext, owner,
            CardPoolSelectCmd.OriginalCharacterPools,
            SharedChoosePoolPrompt);
        if (pool == null) return;

        var powerCards = pool
            .GetUnlockedCards(owner.UnlockState, owner.RunState.CardMultiplayerConstraint)
            .Where(c => c.Type == CardType.Power);

        var (cw, uw, rw) = WeightedCardSelectCmd.GetConfiguredWeights();
        var chosen = await WeightedCardSelectCmd.PickFromCards(
            choiceContext, owner, powerCards,
            commonWeight: cw, uncommonWeight: uw, rareWeight: rw,
            prompt: SharedChooseCardPrompt,
            upgradeOffered: CurrentUpgradeLevel > 0);
        if (chosen == null) return;

        await CardPileCmd.AddGeneratedCardToCombat(chosen, PileType.Hand, creator: chosen.Owner);
    }

    protected override void OnUpgrade()
    {
        SetChronoMarkCost(1);
    }
}
