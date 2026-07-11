using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using SpaceTimeWitch.Cards;
using SpaceTimeWitch.Character;
using SpaceTimeWitch.Commands;
using STS2RitsuLib.Interop.AutoRegistration;

namespace SpaceTimeWitch.Extension.DCB.Tier3;

[RegisterCard(typeof(SpaceTimeWitchExCardPool))]
public class BlisteringBlades3 : SpaceTimeWitchCards
{
    protected override HashSet<CardTag> CanonicalTags =>
    [
        CardTags.DCB3
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(10m,ValueProp.Move)
    ];
    
    protected override IEnumerable<IHoverTip> CardSpecificHoverTips =>
    [
        HoverTipFactory.FromCard<STWMirageBlades>(),
    ];

    public BlisteringBlades3()
        : base(
            baseCost:1,
            type: CardType.Attack,
            rarity: CardRarity.Uncommon,
            target: TargetType.AnyEnemy
        )
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var owner = Owner;
        if (owner?.Creature == null) return;

        ArgumentNullException.ThrowIfNull(play.Target, "cardPlay.Target");

        // 对目标敌人造成伤害
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .Targeting(play.Target!)
            .Execute(choiceContext);

        // 收集个人空间中所有的 STWMirageBlades
        var psBlades = PersonalSpaceCmd.GetCards(owner)
            .OfType<STWMirageBlades>()
            .ToList();

        if (psBlades.Count == 0) return;

        var target = play.Target!;
        if (target.IsDead) return;

        // 对同一目标触发每把幻影剑（直接造成伤害，不消耗，留在个人空间）
        foreach (var blade in psBlades)
        {
            await CreatureCmd.Damage(choiceContext, target,
                blade.DynamicVars.Damage.BaseValue, ValueProp.Unpowered,
                owner.Creature, blade);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3m);
    }

    protected override string PortraitPath => "res://images/Extension/Cards/BlisteringBlades.png";
}
