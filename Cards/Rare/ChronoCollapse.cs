using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using SpaceTimeWitch.Character;
using SpaceTimeWitch.Commands;
using STS2RitsuLib.Interop.AutoRegistration;
using STWSingularity = SpaceTimeWitch.Cards.Token.STWSingularity;

namespace SpaceTimeWitch.Cards.Rare;

[RegisterCard(typeof(SpaceTimeWitchCardPool))]
public class ChronoCollapse : SpaceTimeWitchCards
{
    protected override HashSet<CardTag> CanonicalTags => [];
    protected override IEnumerable<DynamicVar> CanonicalVars => [];

    public ChronoCollapse()
        : base(
            baseCost: 3,
            type: CardType.Skill,
            rarity: CardRarity.Rare,
            target: TargetType.Self
        )
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
    
    
    protected override IEnumerable<IHoverTip> CardSpecificHoverTips =>
    [
        HoverTipFactory.FromCard<STWSingularity>(),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var owner = Owner;
        if (owner?.Creature == null) return;

        // ── 1. 收集手牌 + 抽牌堆 + 弃牌堆 + 个人空间（排除自身）──
        var handCards = PileType.Hand.GetPile(owner).Cards.Where(c => c != this).ToList();
        var drawCards = PileType.Draw.GetPile(owner).Cards.ToList();
        var discardCards = PileType.Discard.GetPile(owner).Cards.ToList();
        var psCards = PersonalSpaceCmd.GetCards(owner).ToList();
        var allCards = handCards.Concat(drawCards).Concat(discardCards).Concat(psCards).ToList();

        int exhaustedCount = allCards.Count;

        // ── 2. 消耗全部 ──
        // 个人空间内的卡牌需要先取出再消耗
        foreach (var card in psCards)
            await PersonalSpaceCmd.Retrieve(owner, card);
        foreach (var card in allCards)
            await CardCmd.Exhaust(choiceContext, card);

        if (exhaustedCount == 0) return;

        // ── 3. 创建 Singularity 衍生物 ──
        var token = (STWSingularity)CombatState.CreateCard<STWSingularity>(owner);
        token.RemainingUses = exhaustedCount;
        await CardPileCmd.AddGeneratedCardToCombat(token, PileType.Hand, creator: token.Owner);

        // ── 4. 施加奇点能力 ──
        await PowerCmd.Apply<Powers.STWSingularityPower>(choiceContext, owner.Creature, exhaustedCount, owner.Creature, this);

        // ── 5. 已升级 → 立即触发一次（消耗次数）──
        if (IsUpgraded)
        {
            var power = owner.Creature.GetPower<Powers.STWSingularityPower>();
            if (power != null)
            {
                await Powers.STWSingularityPower.TriggerOnce(choiceContext, owner);
                await PowerCmd.Decrement(power);
                token.RemainingUses = power.Amount;

                // 触发后若次数归零，清理卡牌和能力
                if (power.Amount <= 0)
                {
                    var hand = PileType.Hand.GetPile(owner);
                    var tokenInHand = hand.Cards.OfType<STWSingularity>().FirstOrDefault();
                    if (tokenInHand != null)
                        await CardCmd.Exhaust(choiceContext, tokenInHand);
                    await PowerCmd.Remove(power);
                }
            }
        }
    }

    protected override void OnUpgrade()
    {
    }
}
