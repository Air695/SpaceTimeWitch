using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using SpaceTimeWitch.Character;
using SpaceTimeWitch.Commands;
using STS2RitsuLib.Interop.AutoRegistration;

namespace SpaceTimeWitch.Cards.Uncommon;

[RegisterCard(typeof(SpaceTimeWitchCardPool))]
public class ChronoArchive : SpaceTimeWitchCards
{
    private static readonly LocString ArchivePrompt = new("cards", "SPACE_TIME_WITCH_CARD_CHRONO_ARCHIVE.selectionScreenPrompt");

    protected override HashSet<CardTag> CanonicalTags =>
    [
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CardsVar(3)
    ];

    // ChronoMark cost set via SetChronoMarkCost(2) in constructor

    public ChronoArchive()
        : base(
            baseCost: 1,
            type: CardType.Skill,
            rarity: CardRarity.Uncommon,
            target: TargetType.Self
        )
    {
        SetChronoMarkCost(2);
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var owner = Owner;
        if (owner?.Creature == null) return;
        
        var allCards = PileType.Hand.GetPile(Owner).Cards
            .Concat(PileType.Draw.GetPile(Owner).Cards)
            .Concat(PileType.Discard.GetPile(Owner).Cards)
            .Where(c => c != this)
            .ToList();
        
        if (allCards.Count == 0)
            return;
        
        var maxSelect = DynamicVars.Cards.IntValue;

        var selected = await CardSelectCmd.FromSimpleGrid(
            choiceContext,
            allCards,
            Owner,
            new CardSelectorPrefs(ArchivePrompt, 0, maxSelect)
        );
            
        foreach (var card in selected)
        {
            await PersonalSpaceCmd.Store(Owner, card);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Cards.UpgradeValueBy(1m);
    }
}