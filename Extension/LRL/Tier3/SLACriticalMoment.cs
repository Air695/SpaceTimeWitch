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

namespace SpaceTimeWitch.Extension.LRL.Tier3;

[RegisterCard(typeof(SpaceTimeWitchExCardPool))]
public class SLACriticalMoment : SpaceTimeWitchCards
{
    protected override HashSet<CardTag> CanonicalTags =>
    [
        CardTags.LRL3
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(20m, ValueProp.Move),
        new BlockVar(30m, ValueProp.Unpowered),
        new PowerVar<STWBleed>(6m)
    ];


    public SLACriticalMoment()
        : base(
            baseCost:2,
            type: CardType.Attack,
            rarity: CardRarity.Uncommon,
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

        var cardPower = DynamicVars.Damage.BaseValue;

        await IndividualHelper.Clash(
            card: this,
            target: target,
            cardPower: cardPower,
            ctx: choiceContext,
            clashProps: ValueProp.Move,
            onVictory: async () =>
            {
                // 胜利：造成 25 伤害 + 6 流血（差值伤害由 IndividualHelper 自动打出）
                await DamageCmd.Attack(cardPower)
                    .FromCard(this)
                    .Targeting(target)
                    .Execute(choiceContext);

                if (target.IsAlive)
                {
                    await PowerCmd.Apply<STWBleed>(
                        choiceContext, target,
                        DynamicVars["STWBleed"].IntValue,
                        owner.Creature, this);
                }
            },
            onDefeat: async () =>
            {
                // 失败：获得格挡 + 6 流血
                await CreatureCmd.GainBlock(
                    owner.Creature,
                    DynamicVars.Block.BaseValue,
                    ValueProp.Unpowered, null);

                if (target.IsAlive)
                {
                    await PowerCmd.Apply<STWBleed>(
                        choiceContext, target,
                        DynamicVars["STWBleed"].IntValue,
                        owner.Creature, this);
                }
            });
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(5m);
        DynamicVars.Block.UpgradeValueBy(5m);
        DynamicVars["STWBleed"].UpgradeValueBy(2m);
    }

    protected override string PortraitPath => $"res://images/Extension/Cards/{GetType().Name}.png";
}