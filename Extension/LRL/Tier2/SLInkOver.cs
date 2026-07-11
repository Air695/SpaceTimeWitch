using System.Linq;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using SpaceTimeWitch.Character;
using SpaceTimeWitch.Cards;
using SpaceTimeWitch.Extension.Powers;

namespace SpaceTimeWitch.Extension.LRL.Tier2;

[RegisterCard(typeof(SpaceTimeWitchExCardPool))]
public class SLInkOver : SpaceTimeWitchCards
{
    private class BleedCalcVar : CalculatedVar
    {
        public BleedCalcVar() : base("CalculatedBleed") { }
        protected override DynamicVar GetBaseVar() => ((CardModel)_owner).DynamicVars["BleedBase"];
        protected override DynamicVar GetExtraVar() => ((CardModel)_owner).DynamicVars["BleedExtra"];
    }

    protected override HashSet<CardTag> CanonicalTags =>
    [
        CardTags.LRL2
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CalculationBaseVar(20m),
        new ExtraDamageVar(2m),
        new CalculatedDamageVar(ValueProp.Move).WithMultiplier((card, _) =>
            OtherHandCount(card)),
        new DynamicVar("BleedBase", 5m),
        new DynamicVar("BleedExtra", 1m),
        new BleedCalcVar().WithMultiplier((card, _) =>
            OtherHandCount(card))
    ];

    /// <summary>手牌中除自身以外的卡牌数量。自身不在手牌时（如从抽牌堆打出）则不减。</summary>
    private static int OtherHandCount(CardModel card)
    {
        var hand = PileType.Hand.GetPile(card.Owner).Cards;
        return Math.Max(0, hand.Count - (hand.Any(c => c == card) ? 1 : 0));
    }

    protected override IEnumerable<IHoverTip> CardSpecificHoverTips =>
    [
        HoverTipFactory.FromPower<STWBleed>(),
    ];

    public SLInkOver()
        : base(
            baseCost:3,
            type: CardType.Attack,
            rarity: CardRarity.Rare,
            target: TargetType.AnyEnemy
        )
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var owner = Owner;
        var target = play.Target;
        if (owner?.Creature == null || target == null) return;

        // 造成伤害（20 + 其他手牌数 × 2）
        await DamageCmd.Attack(DynamicVars.CalculatedDamage)
            .FromCard(this)
            .Targeting(target)
            .Execute(choiceContext);

        // 给予流血（5 + 其他手牌数 × 1）
        var otherCards = OtherHandCount(this);
        var bleedAmount = DynamicVars["BleedBase"].IntValue
            + DynamicVars["BleedExtra"].IntValue * otherCards;
        await PowerCmd.Apply<STWBleed>(
            choiceContext, target, bleedAmount, owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.CalculationBase.UpgradeValueBy(5m);
        DynamicVars.ExtraDamage.UpgradeValueBy(1m);
        DynamicVars["BleedBase"].UpgradeValueBy(3m);
    }

    protected override string PortraitPath => $"res://images/Extension/Cards/{GetType().Name}.png";
}
