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

namespace SpaceTimeWitch.Extension.LRL.Tier2;

[RegisterCard(typeof(SpaceTimeWitchExCardPool))]
public class SLScatteringSlash : SpaceTimeWitchCards
{
    protected override HashSet<CardTag> CanonicalTags =>
    [
        CardTags.LRL2
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(4m,ValueProp.Unpowered),
        new DamageVar(15,ValueProp.Move)
    ];


    public SLScatteringSlash()
        : base(
            baseCost:2,
            type: CardType.Attack,
            rarity: CardRarity.Uncommon,
            target: TargetType.AllEnemies
        )
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [ExK.Summation];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var owner = Owner;
        if (owner?.Creature == null) return;

        var targets = CombatState.HittableEnemies.ToList();
        if (targets.Count == 0) return;

        // 对所有敌人造成 15 点伤害（无条件）
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .TargetingAllOpponents(CombatState)
            .Execute(choiceContext);

        // 拼点：胜利获得格挡
        if (!Keywords.Contains(ExK.Summation)) return;

        await SummationHelper.ClashAll(
            card: this,
            targets: targets,
            baseValue: DynamicVars.Damage.BaseValue,
            repeatCount: 1,
            ctx: choiceContext,
            clashProps: ValueProp.Move,
            onVictory: async (target) =>
            {
                await CreatureCmd.GainBlock(owner.Creature, DynamicVars.Block, play);
            });
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(1m);
        DynamicVars.Damage.UpgradeValueBy(3m);
    }

    protected override string PortraitPath => $"res://images/Extension/Cards/{GetType().Name}.png";
}