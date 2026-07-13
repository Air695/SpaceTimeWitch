using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using SpaceTimeWitch.Character;
using SpaceTimeWitch.Commands;
using SpaceTimeWitch.Scripts;
using STS2RitsuLib.Combat.SecondaryResources;
using STS2RitsuLib.Interop.AutoRegistration;

namespace SpaceTimeWitch.Cards.Uncommon;

[RegisterCard(typeof(SpaceTimeWitchCardPool))]
public class ChronoExtract : SpaceTimeWitchCards
{
    private static readonly LocString ExtractPrompt = new("cards", "SPACE_TIME_WITCH_CARD_CHRONO_EXTRACT.selectionScreenPrompt");

    protected override HashSet<CardTag> CanonicalTags =>
    [
        CardTags.MarkA
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        SecondaryResourceVars.For("ChronoMark", ModChronoResources.Id, 2)
    ];


    public ChronoExtract()
        : base(
            baseCost: 0,
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
        
        var card = (await CardSelectCmd.FromHand(
            prefs: new CardSelectorPrefs(ExtractPrompt, 1),
            context: choiceContext,
            player: owner,
            filter: null,
            source: this
        )).FirstOrDefault();

        if (card != null)
        {
            await CardCmd.Exhaust(choiceContext, card);
        }
        var amount = DynamicVars["ChronoMark"].IntValue;
        await ChronoMark.Gain(owner.Creature, amount);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["ChronoMark"].UpgradeValueBy(1);
    }
}