using System.Linq;
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
using SpaceTimeWitch.Extension.ExKeyWords;
using SpaceTimeWitch.Extension.Powers;

namespace SpaceTimeWitch.Extension.LRL.Tier3;

[RegisterCard(typeof(SpaceTimeWitchExCardPool))]
public class SLDisgorgeInnards : SpaceTimeWitchCards
{
    protected override HashSet<CardTag> CanonicalTags =>
    [
        CardTags.LRL3
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CalculationBaseVar(10m),
        new ExtraDamageVar(1m),
        new CalculatedDamageVar(ValueProp.Move).WithMultiplier((card, target) =>
            target?.Powers.OfType<STWBleed>().FirstOrDefault()?.Amount ?? 0),
        new CardsVar(2)
    ];


    public SLDisgorgeInnards()
        : base(
            baseCost:3,
            type: CardType.Attack,
            rarity: CardRarity.Rare,
            target: TargetType.AnyEnemy
        )
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [ExK.Individual];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var owner = Owner;
        var target = play.Target;
        if (owner?.Creature == null || target == null) return;

        if (!Keywords.Contains(ExK.Individual)) return;

        for (int i = 0; i < DynamicVars.Cards.IntValue; i++)
        {
            if (!target.IsAlive) break;

            var cardPower = DynamicVars.CalculatedDamage.Calculate(target);

            // 每段拼点（仅差值伤害，无额外效果）
            await IndividualHelper.Clash(
                card: this,
                target: target,
                cardPower: cardPower,
                ctx: choiceContext,
                clashProps: ValueProp.Move);

            // 造成伤害（10 + 流血加成）
            if (target.IsAlive)
            {
                await DamageCmd.Attack(DynamicVars.CalculatedDamage)
                    .FromCard(this)
                    .Targeting(target)
                    .Execute(choiceContext);
            }
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.ExtraDamage.UpgradeValueBy(1m);
    }

    protected override string PortraitPath => $"res://images/Extension/Cards/{GetType().Name}.png";
}