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
public class SLIngredientHunt : SpaceTimeWitchCards
{
    protected override HashSet<CardTag> CanonicalTags =>
    [
        CardTags.LRL1
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(16m,ValueProp.Move),
        new PowerVar<STWBleed>(5m)
    ];


    public SLIngredientHunt()
        : base(
            baseCost:2,
            type: CardType.Attack,
            rarity: CardRarity.Common,
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
                // 拼点成功：给予 5 流血
                await PowerCmd.Apply<STWBleed>(
                    choiceContext, target,
                    DynamicVars["STWBleed"].IntValue,
                    owner.Creature, this);
            });

        // 造成 16 点伤害
        if (target.IsAlive)
        {
            await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
                .FromCard(this)
                .Targeting(target)
                .Execute(choiceContext);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(4m);
        DynamicVars["STWBleed"].UpgradeValueBy(2m);
    }

    protected override string PortraitPath => $"res://images/Extension/Cards/{GetType().Name}.png";
}