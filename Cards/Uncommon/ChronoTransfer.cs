using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using SpaceTimeWitch.Character;
using STS2RitsuLib.Interop.AutoRegistration;

namespace SpaceTimeWitch.Cards.Uncommon;

[RegisterCard(typeof(SpaceTimeWitchCardPool))]
public class ChronoTransfer : SpaceTimeWitchCards
{
    protected override HashSet<CardTag> CanonicalTags =>
    [
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CardsVar(2)
    ];

    public ChronoTransfer()
        : base(
            baseCost:0,
            type: CardType.Skill,
            rarity: CardRarity.Uncommon,
            target: TargetType.AnyAlly
        )
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [];
    
    public override CardMultiplayerConstraint MultiplayerConstraint => 
        CardMultiplayerConstraint.MultiplayerOnly;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var owner = Owner;
        if (owner?.Creature == null) return;

        // 确认目标为有效队友
        var targetCreature = play.Target;
        if (targetCreature == null || !targetCreature.IsPlayer || targetCreature == owner.Creature)
            return;

        var teammate = targetCreature.Player;
        if (teammate == null) return;

        // 收集手牌、抽牌堆、弃牌堆
        var allCards = PileType.Hand.GetPile(owner).Cards
            .Concat(PileType.Draw.GetPile(owner).Cards)
            .Concat(PileType.Discard.GetPile(owner).Cards)
            .ToList();

        if (allCards.Count == 0) return;

        var maxSelect = DynamicVars.Cards.IntValue;

        var selected = await CardSelectCmd.FromSimpleGrid(
            choiceContext, allCards, owner,
            new CardSelectorPrefs(
                new LocString("cards", "STW_CHRONO_DELIVER_PROMPT"),
                minCount: 0, maxCount: maxSelect));

        var cs = CombatState;

        foreach (var card in selected)
        {
            // 为队友创建 CombatState 副本，保留卡牌的所有修改状态
            var combatClone = cs.CloneCard(card);
            cs.RemoveCard(combatClone); // 清除 CloneCard 保留的原所有者
            cs.AddCard(combatClone, teammate);

            if (IsUpgraded)
                CardCmd.Upgrade(combatClone);

            // 持久化到队友的 RunState 并建立 DeckVersion 关联，
            // 避免抽到时因为 DeckVersion 指向原持有者 RunState 而导致卡死
            var runStateClone = teammate.RunState.CloneCard(combatClone);
            combatClone.DeckVersion = runStateClone;

            // 将 CombatState 副本加入队友手牌
            await CardPileCmd.Add(combatClone, PileType.Hand);

            // 从原持有者处移除原卡牌（不触发消耗效果）
            await CardPileCmd.RemoveFromCombat(card);
        }
    }

    protected override void OnUpgrade()
    {
    }
}