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
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;

namespace SpaceTimeWitch.Extension.QG.WXC;

[RegisterCard(typeof(STWEGO))]
public class SLRedEyes : SpaceTimeWitchCards, IEGOCard
{
    public CardTag Tag => CardTags.WXCE;

    protected override HashSet<CardTag> CanonicalTags =>
    [
        CardTags.WXCE
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(28,ValueProp.Move),
        new PowerVar<VulnerablePower>(3m),
        new PowerVar<WeakPower>(3m),
        new PowerVar<FrailPower>(3m),
        new PowerVar<STWBleed>(5m)
    ];
    
    protected override IEnumerable<IHoverTip> CardSpecificHoverTips =>
    [
        new HoverTip(
            new LocString("cards", "NOPE"),
            new LocString("cards", "SL_RED_EYES")
        ),
    ];


    public SLRedEyes()
        : base(
            baseCost:2,
            type: CardType.Attack,
            rarity: CardRarity.Rare,
            target: TargetType.AnyEnemy
        )
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust,CardKeyword.Retain];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var target = play.Target;
        if (target == null) return;

        // 造成伤害
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .Targeting(target)
            .Execute(choiceContext);

        if (!target.IsAlive) return;

        // 给予 debuff
        await PowerCmd.Apply<VulnerablePower>(
            choiceContext, target, DynamicVars["VulnerablePower"].IntValue,
            Owner.Creature, this);
        await PowerCmd.Apply<WeakPower>(
            choiceContext, target, DynamicVars["WeakPower"].IntValue,
            Owner.Creature, this);
        await PowerCmd.Apply<FrailPower>(
            choiceContext, target, DynamicVars["FrailPower"].IntValue,
            Owner.Creature, this);
        await PowerCmd.Apply<STWBleed>(
            choiceContext, target, DynamicVars["STWBleed"].IntValue,
            Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(5m);
        DynamicVars["VulnerablePower"].UpgradeValueBy(2m);
        DynamicVars["WeakPower"].UpgradeValueBy(2m);
        DynamicVars["FrailPower"].UpgradeValueBy(2m);
        DynamicVars["STWBleed"].UpgradeValueBy(3m);
    }

    protected override string PortraitPath => $"res://images/Extension/EGO/{GetType().Name}.png";
}