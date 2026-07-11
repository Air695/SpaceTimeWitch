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

namespace SpaceTimeWitch.Extension.LRL.Tier2;

[RegisterCard(typeof(SpaceTimeWitchExCardPool))]
public class SLSkyClearingCut : SpaceTimeWitchCards
{
    protected override HashSet<CardTag> CanonicalTags =>
    [
        CardTags.LRL2
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(17m, ValueProp.Move),
        new PowerVar<STWBleed>(4m),
        new PowerVar<STWBleedN>(4m)
    ];


    public SLSkyClearingCut()
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

        await IndividualHelper.Clash(
            card: this,
            target: target,
            cardPower: DynamicVars.Damage.BaseValue,
            ctx: choiceContext,
            clashProps: ValueProp.Move,
            onVictory: async () =>
            {
                // 拼点成功：下回合给予 4 流血
                await PowerCmd.Apply<STWBleedN>(
                    choiceContext, target,
                    DynamicVars["STWBleedN"].IntValue,
                    owner.Creature, this);
            });

        // 造成 17 点伤害，给予 4 流血
        if (target.IsAlive)
        {
            await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
                .FromCard(this)
                .Targeting(target)
                .Execute(choiceContext);

            await PowerCmd.Apply<STWBleed>(
                choiceContext, target,
                DynamicVars["STWBleed"].IntValue,
                owner.Creature, this);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3m);
        DynamicVars["STWBleed"].UpgradeValueBy(2m);
        DynamicVars["STWBleedN"].UpgradeValueBy(2m);
    }

    protected override string PortraitPath => $"res://images/Extension/Cards/{GetType().Name}.png";
}