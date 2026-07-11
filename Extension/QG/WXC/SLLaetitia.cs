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
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using STS2RitsuLib.Keywords;

namespace SpaceTimeWitch.Extension.QG.WXC;

[RegisterCard(typeof(STWEGO))]
public class SLLaetitia : SpaceTimeWitchCards, IEGOCard
{
    public CardTag Tag => CardTags.WXCE;

    protected override HashSet<CardTag> CanonicalTags =>
    [
        CardTags.WXCE
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(20m,ValueProp.Move)
    ];
    
    protected override IEnumerable<IHoverTip> CardSpecificHoverTips =>
    [
        new HoverTip(
            new LocString("cards", "NOPE"),
            new LocString("cards", "SL_LAETITIA")
        ),
    ];


    public SLLaetitia()
        : base(
            baseCost:2,
            type: CardType.Attack,
            rarity: CardRarity.Rare,
            target: TargetType.AllEnemies
        )
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust, CardKeyword.Retain, ExK.Summation];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var owner = Owner;
        if (owner?.Creature == null) return;

        var enemies = owner.Creature.CombatState.HittableEnemies.ToList();

        // ① 先清算拼点
        if (Keywords.Contains(ExK.Summation))
        {
            await SummationHelper.ClashAll(
                card: this,
                targets: enemies,
                baseValue: DynamicVars.Damage.IntValue,
                repeatCount: 1,
                ctx: choiceContext,
                onVictory: async (target) =>
                {
                    await PowerCmd.Apply<SLLaetitiaP>(
                        choiceContext, target, 1, owner.Creature, this);
                });
        }

        // ② 再对所有敌人造成伤害
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .TargetingAllOpponents(owner.Creature.CombatState)
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(5m);
    }

    protected override string PortraitPath => $"res://images/Extension/EGO/{GetType().Name}.png";
}