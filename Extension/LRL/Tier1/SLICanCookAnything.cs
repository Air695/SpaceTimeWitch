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
using SpaceTimeWitch.Extension.Powers;
using SpaceTimeWitch.Extension.ExKeyWords;

namespace SpaceTimeWitch.Extension.LRL.Tier1;

[RegisterCard(typeof(SpaceTimeWitchExCardPool))]
public class SLICanCookAnything : SpaceTimeWitchCards
{
    protected override HashSet<CardTag> CanonicalTags =>
    [
        CardTags.LRL1
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CalculationBaseVar(15m),
        new ExtraDamageVar(1m),
        new CalculatedDamageVar(ValueProp.Move).WithMultiplier((card, target) =>
            target?.Powers.OfType<STWBleed>().FirstOrDefault()?.Amount ?? 0)
    ];


    public SLICanCookAnything()
        : base(
            baseCost:2,
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
        if (owner?.Creature == null) return;

        var target = play.Target;
        if (target == null) return;

        if (!Keywords.Contains(ExK.Individual)) return;

        // 拼点
        var cardPower = DynamicVars.CalculatedDamage.Calculate(target);

        await IndividualHelper.Clash(
            card: this,
            target: target,
            cardPower: cardPower,
            ctx: choiceContext,
            clashProps: ValueProp.Move,
            onVictory: async () =>
            {
                // 拼点成功：回复 15% 伤害的生命值
                var healAmount = (int)(cardPower * 0.15m);
                if (healAmount > 0)
                {
                    await CreatureCmd.Heal(owner.Creature, healAmount);
                }
            });

        // 造成 15 点伤害，目标每有 1 流血 +1
        if (target.IsAlive)
        {
            await DamageCmd.Attack(DynamicVars.CalculatedDamage)
                .FromCard(this)
                .Targeting(target)
                .Execute(choiceContext);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.CalculationBase.UpgradeValueBy(5m);
        DynamicVars.ExtraDamage.UpgradeValueBy(1m);
    }

    protected override string PortraitPath => $"res://images/Extension/Cards/{GetType().Name}.png";
}