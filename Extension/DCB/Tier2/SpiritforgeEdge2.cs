using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using SpaceTimeWitch.Cards;
using SpaceTimeWitch.Character;
using STS2RitsuLib.Interop.AutoRegistration;

namespace SpaceTimeWitch.Extension.DCB.Tier2;

[RegisterCard(typeof(SpaceTimeWitchExCardPool))]
public class SpiritforgeEdge2 : SpaceTimeWitchCards
{
    protected override HashSet<CardTag> CanonicalTags =>
    [
        CardTags.DCB2
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
    ];
    
    protected override IEnumerable<IHoverTip> CardSpecificHoverTips =>
    [
        HoverTipFactory.FromCard<STWMirageBlades>(),
    ];

    public SpiritforgeEdge2()
        : base(
            baseCost:0,
            type: CardType.Skill,
            rarity: CardRarity.Uncommon,
            target: TargetType.Self
        )
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Retain];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var owner = Owner;
        if (owner?.Creature == null) return;

        var selected = await CardSelectCmd.FromHand(
            prefs: new CardSelectorPrefs(SharedChooseCardPrompt, 1, 1),
            context: choiceContext,
            player: owner,
            filter: c => c.Type == CardType.Attack,
            source: this
        );

        var chosen = selected.FirstOrDefault();
        if (chosen == null) return;

        var damage = chosen.DynamicVars.TryGetValue("Damage", out var dv) ? dv.BaseValue : 0m;

        await CardCmd.Exhaust(choiceContext, chosen);

        if (IsUpgraded)
            damage *= 2m;
        else
            damage *= 1.5m;

        var blade = (STWMirageBlades)CombatState.CreateCard<STWMirageBlades>(owner);
        blade.DynamicVars.Damage.BaseValue = damage;
        await CardPileCmd.AddGeneratedCardToCombat(blade, PileType.Hand, creator: owner);
    }

    protected override void OnUpgrade()
    {
    }

    protected override string PortraitPath => "res://images/Extension/Cards/SpiritforgeEdge.png";
}
