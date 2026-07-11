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
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;

namespace SpaceTimeWitch.Extension.QG.WXC;

[RegisterCard(typeof(STWEGO))]
public class SLSanguineDesire : SpaceTimeWitchCards, IEGOCard
{
    public CardTag Tag => CardTags.WXCE;

    protected override HashSet<CardTag> CanonicalTags =>
    [
        CardTags.WXCE
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(6m,ValueProp.Move),
        new CardsVar(4)
    ];
    
    protected override IEnumerable<IHoverTip> CardSpecificHoverTips =>
    [
        new HoverTip(
            new LocString("cards", "NOPE"),
            new LocString("cards", "SL_SANGUINE_DESIRE")
        ),
    ];


    public SLSanguineDesire()
        : base(
            baseCost:3,
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

        // 先施加 SLSanguineDesireP，使流血在后续多段攻击中不会削减
        await PowerCmd.Apply<SLSanguineDesireP>(
            choiceContext, target, 1, Owner.Creature, this);

        var hitCount = DynamicVars.Cards.IntValue;
        for (int i = 0; i < hitCount; i++)
        {
            // ① 造成一段攻击伤害
            await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
                .FromCard(this)
                .Targeting(target)
                .Execute(choiceContext);

            if (!target.IsAlive) break;

            // ② 引爆流血（SLSanguineDesireP 保护下层数不降）
            await STWBleed.Detonate(target, choiceContext, Owner.Creature, this);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Cards.UpgradeValueBy(1);
    }

    protected override string PortraitPath => $"res://images/Extension/EGO/{GetType().Name}.png";
}