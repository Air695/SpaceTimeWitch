using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using SpaceTimeWitch.Cards;
using SpaceTimeWitch.Cards.KeyWords;
using STS2RitsuLib.Keywords;

namespace SpaceTimeWitch.CombatSkill;

/// <summary>
/// CombatSkill 卡牌基类。
/// 数值在 CanonicalVars，完整行动（核心+额外效果）在 GetActionData()。
/// 替换时数值保留本卡，行动整套换成上一张的。
/// </summary>
public abstract class CombatSkillCard : SpaceTimeWitchCards
{
    /// <summary>此卡的完整行动（核心动作 + 额外效果）</summary>
    public abstract CombatSkillActionData GetActionData();

    protected override HashSet<CardTag> CanonicalTags => [];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [
        STWKeywords.CombatSkill.GetModKeywordCardKeyword()
    ];

    protected override IEnumerable<string> RegisteredKeywordIds => [STWKeywords.CombatSkill];

    internal IEnumerable<DynamicVar> ExposedCanonicalVars => CanonicalVars;

    /// <summary>施加副值能力。executor 传入 action 决定的目标，子卡按需覆写。</summary>
    public virtual async Task ApplyPowers(Creature target, PlayerChoiceContext ctx) { }

    protected CombatSkillCard(int baseCost, CardType type, CardRarity rarity)
        : base(baseCost, type, rarity, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var owner = Owner;
        if (owner?.Creature == null) return;

        var state = CombatSkillManager.GetState(Owner);

        // 激活替换时用上一张 CS 卡的完整行动，否则用本卡自己的
        var actionData = (state.IsSwapActive && state.LastAction != null)
            ? state.LastAction
            : GetActionData();

        await CombatSkillExecutor.Execute(actionData, this, choiceContext, cardPlay);

        state.LastAction = GetActionData(); // 记住本卡原始行动
        state.IsSwapActive = false;
    }
}
