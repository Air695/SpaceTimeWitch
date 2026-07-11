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
using SpaceTimeWitch.Powers;
using SpaceTimeWitch.Extension.ExKeyWords;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;

namespace SpaceTimeWitch.Extension.QG.WXC;

[RegisterCard(typeof(STWEGO))]
public class SLBlackSwan : SpaceTimeWitchCards, IEGOCard
{
    public CardTag Tag => CardTags.WXCE;

    protected override HashSet<CardTag> CanonicalTags =>
    [
        CardTags.WXCE
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(30m,ValueProp.Move),
        new PowerVar<TSP>(8m),
        new PowerVar<TTSP>(8m)
    ];
    
    protected override IEnumerable<IHoverTip> CardSpecificHoverTips =>
    [
        new HoverTip(
            new LocString("cards", "NOPE"),
            new LocString("cards", "SL_BLACK_SWAN")
        ),
    ];


    public SLBlackSwan()
        : base(
            baseCost:3,
            type: CardType.Attack,
            rarity: CardRarity.Rare,
            target: TargetType.AllEnemies
        )
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust,CardKeyword.Retain,ExK.Summation];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var owner = Owner;
        if (owner?.Creature == null) return;

        var enemies = owner.Creature.CombatState.HittableEnemies.ToList();

        // ① Summation 清算拼点
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
                    await PowerCmd.Apply<TSP>(
                        choiceContext, target,
                        -DynamicVars["TSP"].IntValue,
                        owner.Creature, this);
                    await PowerCmd.Apply<TTSP>(
                        choiceContext, target,
                        -DynamicVars["TTSP"].IntValue,
                        owner.Creature, this);
                });
        }

        // ② 群体伤害
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .TargetingAllOpponents(owner.Creature.CombatState)
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(10m);
        DynamicVars["TSP"].UpgradeValueBy(2m);
        DynamicVars["TTSP"].UpgradeValueBy(2m);
    }

    protected override string PortraitPath => $"res://images/Extension/EGO/{GetType().Name}.png";
}