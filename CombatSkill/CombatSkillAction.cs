using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace SpaceTimeWitch.CombatSkill;

/// <summary>核心动作类型</summary>
public enum CombatSkillCoreAction
{
    Strike,
    StrikeAll,
    StrikeRandom,
    Defend,
    DefendAll,
}

/// <summary>完整行动 = 核心动作 + 额外效果。替换时整套交换。</summary>
public class CombatSkillActionData
{
    public CombatSkillCoreAction CoreAction;
    public int HitCount = 1;
    public Func<CombatSkillCard, PlayerChoiceContext, CardPlay, Task>? ExtraEffect;
}

/// <summary>
/// 根据 CombatSkillActionData 执行卡牌效果。
/// </summary>
public static class CombatSkillExecutor
{

    public static async Task Execute(
        CombatSkillActionData actionData,
        CombatSkillCard card,
        PlayerChoiceContext ctx,
        CardPlay play)
    {
        var creature = card.Owner?.Creature;
        if (creature == null) return;

        var baseValue = GetBaseValue(card);
        bool isNative = (card.Type == CardType.Attack && actionData.CoreAction != CombatSkillCoreAction.Defend)
                     || (card.Type == CardType.Skill && actionData.CoreAction == CombatSkillCoreAction.Defend);

        for (int i = 0; i < actionData.HitCount; i++)
        {
            switch (actionData.CoreAction)
            {
                case CombatSkillCoreAction.Strike:
                    await ExecuteStrike(card, ctx, play, baseValue, isNative);
                    break;
                case CombatSkillCoreAction.StrikeAll:
                    await ExecuteStrikeAll(card, ctx, baseValue, isNative);
                    break;
                case CombatSkillCoreAction.StrikeRandom:
                    await ExecuteStrikeRandom(card, ctx, baseValue, isNative);
                    break;
                case CombatSkillCoreAction.Defend:
                    await ExecuteDefend(card, ctx, play, baseValue, isNative);
                    break;
                case CombatSkillCoreAction.DefendAll:
                    await ExecuteDefendAll(card, ctx, play, baseValue, isNative);
                    break;
            }
        }

        if (actionData.ExtraEffect != null)
            await actionData.ExtraEffect(card, ctx, play);
    }

    public static decimal GetBaseValue(CombatSkillCard card)
    {
        if (card.Type == CardType.Attack)
            return card.DynamicVars.Damage.BaseValue;
        return card.DynamicVars.Block.BaseValue;
    }

    public static decimal CalcEffective(CombatSkillCard card, decimal baseValue, Creature? target, CardPlay play)
    {
        var creature = card.Owner!.Creature!;
        if (card.Type == CardType.Attack)
        {
            return Hook.ModifyDamage(
                card.Owner.RunState, creature.CombatState, target, creature,
                baseValue, ValueProp.Move, card,
                ModifyDamageHookType.All, CardPreviewMode.None,
                out _);
        }

        return Hook.ModifyBlock(
            creature.CombatState, creature, baseValue,
            ValueProp.Move, card, play,
            out _);
    }

    private static async Task ExecuteStrike(CombatSkillCard card, PlayerChoiceContext ctx, CardPlay play, decimal baseValue, bool isNative)
    {
        var target = play.Target;
        if (target == null) return;
        var applier = card.Owner!.Creature!;
        if (isNative)
            await DamageCmd.Attack(baseValue).FromCard(card).Targeting(target).Execute(ctx);
        else
        {
            var val = CalcEffective(card, baseValue, null, play);
            await CreatureCmd.Damage(ctx, target, val, ValueProp.Unpowered, applier, card);
        }
        await card.ApplyPowers(target, ctx);
    }

    private static async Task ExecuteStrikeAll(CombatSkillCard card, PlayerChoiceContext ctx, decimal baseValue, bool isNative)
    {
        var applier = card.Owner!.Creature!;
        foreach (var enemy in card.CombatState.HittableEnemies)
        {
            if (isNative)
                await DamageCmd.Attack(baseValue).FromCard(card).Targeting(enemy).Execute(ctx);
            else
            {
                var val = CalcEffective(card, baseValue, null, null!);
                await CreatureCmd.Damage(ctx, enemy, val, ValueProp.Unpowered, applier, card);
            }
            await card.ApplyPowers(enemy, ctx);
        }
    }

    private static async Task ExecuteStrikeRandom(CombatSkillCard card, PlayerChoiceContext ctx, decimal baseValue, bool isNative)
    {
        var enemies = card.CombatState.HittableEnemies.ToList();
        if (enemies.Count == 0) return;
        var target = enemies[card.Owner!.RunState.Rng.CombatCardGeneration.NextInt() % enemies.Count];
        var applier = card.Owner!.Creature!;
        if (isNative)
            await DamageCmd.Attack(baseValue).FromCard(card).Targeting(target).Execute(ctx);
        else
        {
            var val = CalcEffective(card, baseValue, null, null!);
            await CreatureCmd.Damage(ctx, target, val, ValueProp.Unpowered, applier, card);
        }
        await card.ApplyPowers(target, ctx);
    }

    private static async Task ExecuteDefend(CombatSkillCard card, PlayerChoiceContext ctx, CardPlay play, decimal baseValue, bool isNative)
    {
        var creature = card.Owner!.Creature!;
        if (isNative)
        {
            var blockVar = new BlockVar(baseValue, ValueProp.Move);
            await CreatureCmd.GainBlock(creature, blockVar, play);
        }
        else
        {
            var val = CalcEffective(card, baseValue, play.Target, play);
            var blockVar = new BlockVar(val, ValueProp.Unpowered);
            await CreatureCmd.GainBlock(creature, blockVar, play);
        }
        await card.ApplyPowers(creature, ctx);
    }

    private static async Task ExecuteDefendAll(CombatSkillCard card, PlayerChoiceContext ctx, CardPlay play, decimal baseValue, bool isNative)
    {
        foreach (var ally in card.CombatState.Allies)
        {
            if (isNative)
            {
                var blockVar = new BlockVar(baseValue, ValueProp.Move);
                await CreatureCmd.GainBlock(ally, blockVar, play);
            }
            else
            {
                var val = CalcEffective(card, baseValue, play.Target, play);
                var blockVar = new BlockVar(val, ValueProp.Unpowered);
                await CreatureCmd.GainBlock(ally, blockVar, play);
            }
            await card.ApplyPowers(ally, ctx);
        }
    }

}
