using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using SpaceTimeWitch.Character;
using SpaceTimeWitch.Cards;
using SpaceTimeWitch.Extension.Powers;

namespace SpaceTimeWitch.Extension.LRL.Tier2;

[RegisterCard(typeof(SpaceTimeWitchExCardPool))]
public class SLRulesoftheBackstreets : SpaceTimeWitchCards
{
    protected override HashSet<CardTag> CanonicalTags =>
    [
        CardTags.LRL2
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<STWBleed>(2m),
        new CardsVar(2)
    ];


    public SLRulesoftheBackstreets()
        : base(
            baseCost:0,
            type: CardType.Skill,
            rarity: CardRarity.Common,
            target: TargetType.AnyEnemy
        )
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var owner = Owner;
        var target = play.Target;
        if (owner?.Creature == null || target == null) return;

        await PowerCmd.Apply<STWBleed>(
            choiceContext, target,
            DynamicVars["STWBleed"].IntValue,
            owner.Creature, this);

        await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.IntValue, owner);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["STWBleed"].UpgradeValueBy(2m);
        DynamicVars.Cards.UpgradeValueBy(1);
    }

    protected override string PortraitPath => $"res://images/Extension/Cards/{GetType().Name}.png";
}