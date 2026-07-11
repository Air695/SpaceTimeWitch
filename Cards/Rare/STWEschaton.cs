using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using SpaceTimeWitch.Cards.Curse;
using SpaceTimeWitch.Character;
using STS2RitsuLib.Interop.AutoRegistration;

namespace SpaceTimeWitch.Cards.Rare;

[RegisterCard(typeof(SpaceTimeWitchCardPool))]
public class STWEschaton : SpaceTimeWitchCards
{
    protected override HashSet<CardTag> CanonicalTags =>
    [
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
    ];

    public STWEschaton()
        : base(
            baseCost:0,
            type: CardType.Attack,
            rarity: CardRarity.Rare,
            target: TargetType.AllEnemies
        )
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
    
    
    protected override IEnumerable<IHoverTip> CardSpecificHoverTips =>
    [
        HoverTipFactory.FromCard<STWLoneliness>(),
    ];
    
    public override async Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
    {
        if (card != this) return;
        var owner = Owner;
        if (owner?.Creature == null) return;
        await CardPileCmd.Draw(choiceContext, 1, owner);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var owner = Owner;
        if (owner?.Creature == null) return;

        // ① 向构筑卡组添加孤独（CreateCard 进 CombatState，需克隆进 RunState 才能入 Deck）
        var loneliness = owner.Creature.CombatState.CreateCard<STWLoneliness>(owner);
        var deckCard = owner.RunState.CloneCard(loneliness);
        CombatState.RemoveCard(loneliness);
        await CardPileCmd.Add(deckCard, PileType.Deck, clonedBy: this);

        // ② 所有敌人失去25%最大生命值与相等的当前生命值（绕过一切效果）
        foreach (var enemy in CombatState.Enemies.Where(e => !e.IsDead).ToList())
        {
            var reduction = (int)(enemy.MaxHp * 0.34m);
            if (reduction <= 0) continue;

            await CreatureCmd.SetCurrentHp(enemy, Math.Max(0, enemy.CurrentHp - reduction));
            if (enemy.IsDead) continue;

            await CreatureCmd.SetMaxHp(enemy, Math.Max(1, enemy.MaxHp - reduction));
        }

        // ③ 结束所有角色的回合
        PlayerCmd.EndTurn(owner, canBackOut: false);

        foreach (var enemy in CombatState.Enemies.Where(e => !e.IsDead))
        {
            await CreatureCmd.Stun(enemy);
        }  
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Retain);
    }
}