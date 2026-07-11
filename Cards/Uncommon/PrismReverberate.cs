using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using SpaceTimeWitch.Character;
using SpaceTimeWitch.Commands;
using STS2RitsuLib.Interop.AutoRegistration;

namespace SpaceTimeWitch.Cards.Uncommon;

[RegisterCard(typeof(SpaceTimeWitchCardPool))]
public class PrismReverberate : SpaceTimeWitchCards
{
    protected override HashSet<CardTag> CanonicalTags =>
    [
        CardTags.Reproduce
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
    ];


    public PrismReverberate()
        : base(
            baseCost:0,
            type: CardType.Attack,
            rarity: CardRarity.Uncommon,
            target: TargetType.Self
        )
    {
        SetChronoMarkCost(1);
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var owner = Owner;
        if (owner?.Creature == null) return;

        var heavyAttacks = CardPoolSelectCmd.OriginalCharacterPools
            .SelectMany(p => p.GetUnlockedCards(
                owner.UnlockState, owner.RunState.CardMultiplayerConstraint))
            .Distinct()
            .Where(c => c.Type == CardType.Attack
                        && (c.EnergyCost.Canonical >= 3 || c.EnergyCost.CostsX));

        var (cw, uw, rw) = WeightedCardSelectCmd.GetConfiguredWeights();
        var chosen = await WeightedCardSelectCmd.PickFromCards(
            choiceContext, owner, heavyAttacks,
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