using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.ValueProps;
using SpaceTimeWitch.Extension.QG;

namespace SpaceTimeWitch.Extension.ExKeyWords;

/// <summary>
/// Individual（交锋）关键字的拼点逻辑。
/// 卡牌数值 vs 敌人单段攻击数值 → 胜利/失败 → 差值伤害。
/// </summary>
public static class IndividualHelper
{
    /// <param name="isBlock">是否为格挡拼点（走格挡管道），默认 false（走伤害管道）</param>
    public static async Task ClashAll(
        CardModel card,
        IEnumerable<Creature> targets,
        decimal cardPower,
        PlayerChoiceContext ctx,
        ValueProp clashProps = ValueProp.Move,
        Func<Creature, Task>? onVictory = null,
        Func<Creature, Task>? onDefeat = null,
        bool isBlock = false)
    {
        foreach (var target in targets)
        {
            if (!target.IsAlive) continue;
            await Clash(card, target, cardPower, ctx, clashProps,
                onVictory != null ? () => onVictory(target) : null,
                onDefeat != null ? () => onDefeat(target) : null,
                isBlock);
        }
    }

    /// <param name="isBlock">是否为格挡拼点（走格挡管道），默认 false（走伤害管道）</param>
    public static async Task<bool> Clash(
        CardModel card,
        Creature target,
        decimal cardPower,
        PlayerChoiceContext ctx,
        ValueProp clashProps = ValueProp.Move,
        Func<Task>? onVictory = null,
        Func<Task>? onDefeat = null,
        bool isBlock = false)
    {
        if (target.Monster == null) return false;
        if (!target.Monster.IntendsToAttack) return false;

        var attackIntent = target.Monster.NextMove.Intents
            .OfType<AttackIntent>()
            .FirstOrDefault();
        if (attackIntent == null) return false;

        var owner = card.Owner;
        var playerCreature = owner?.Creature;
        if (playerCreature == null) return false;

        // 卡牌数值走对应管道（攻击或格挡）
        var combatState = card.CombatState ?? playerCreature.CombatState;
        var modifiedCardPower = isBlock
            ? Hook.ModifyBlock(combatState, playerCreature,
                cardPower, clashProps, card, null, out _)
            : Hook.ModifyDamage(owner.RunState, combatState,
                target, playerCreature, cardPower, clashProps, card,
                ModifyDamageHookType.All, CardPreviewMode.None, out _);

        var enemyPower = attackIntent.GetSingleDamage(
            new[] { playerCreature }, target);

        // 双方取整后再比拼、计算差值
        var cardPowerInt = (int)decimal.Floor(modifiedCardPower);
        var enemyPowerInt = (int)decimal.Floor(enemyPower);
        bool victory = cardPowerInt >= enemyPowerInt;

        if (victory)
        {
            if (onVictory != null) await onVictory();
            await EmotionSystem.AddPositive(owner, 2);
        }
        else
        {
            if (onDefeat != null) await onDefeat();
            await EmotionSystem.AddNegative(owner, 2);
        }

        // 差值伤害：不受力量影响
        var diff = Math.Abs(cardPowerInt - enemyPowerInt);
        if (diff > 0)
        {
            if (victory)
                await CreatureCmd.Damage(ctx, target, diff, ValueProp.Unpowered, playerCreature, card);
            else
                await CreatureCmd.Damage(ctx, playerCreature, diff, ValueProp.Unpowered, target, card);
        }

        return true;
    }
}
