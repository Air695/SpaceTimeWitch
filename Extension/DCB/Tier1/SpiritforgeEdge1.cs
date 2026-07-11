using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using SpaceTimeWitch.Cards;
using SpaceTimeWitch.Character;
using STS2RitsuLib.Interop.AutoRegistration;

namespace SpaceTimeWitch.Extension.DCB.Tier1;

[RegisterCard(typeof(SpaceTimeWitchExCardPool))]
public class SpiritforgeEdge1 : SpaceTimeWitchCards
{
    protected override HashSet<CardTag> CanonicalTags =>
    [
        CardTags.DCB1
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
    ];
    
    protected override IEnumerable<IHoverTip> CardSpecificHoverTips =>
    [
        HoverTipFactory.FromCard<STWMirageBlades>(),
    ];

    public SpiritforgeEdge1()
        : base(
            baseCost:0,
            type: CardType.Skill,
            rarity: CardRarity.Uncommon,
            target: TargetType.Self
        )
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Retain];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var owner = Owner;
        if (owner?.Creature == null) return;

        // 从手牌中选择一张攻击卡消耗
        var selected = await CardSelectCmd.FromHand(
            prefs: new CardSelectorPrefs(SharedChooseCardPrompt, 1, 1),
            context: choiceContext,
            player: owner,
            filter: c => c.Type == CardType.Attack,
            source: this
        );

        var chosen = selected.FirstOrDefault();
        if (chosen == null) return;

        // 获取该卡牌的伤害值（无Damage动态变量时默认为0）
        var damage = chosen.DynamicVars.TryGetValue("Damage", out var dv) ? dv.BaseValue : 0m;

        // 消耗该卡牌
        await CardCmd.Exhaust(choiceContext, chosen);

        // 基础1.2倍，升级后1.5倍
        if (IsUpgraded)
            damage *= 1.5m;
        else
            damage *= 1.2m;

        // 创建幻影剑
        var blade = (STWMirageBlades)CombatState.CreateCard<STWMirageBlades>(owner);
        blade.DynamicVars.Damage.BaseValue = damage;
        await CardPileCmd.AddGeneratedCardToCombat(blade, PileType.Hand, creator: owner);
    }

    protected override void OnUpgrade()
    {
    }

    protected override string PortraitPath => "res://images/Extension/Cards/SpiritforgeEdge.png";
}
