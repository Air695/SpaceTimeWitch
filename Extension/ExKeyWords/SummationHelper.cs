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

namespace SpaceTimeWitch.Extension.ExKeyWords;

/// <summary>
/// Summation（清算）关键字的拼点逻辑。
/// 卡牌总数值（baseValue × repeatCount）vs 敌人总攻击数值。
/// 无感情、无引爆流血、失败无反伤。
/// </summary>
public static class SummationHelper
{
    /// <param name="isBlock">是否为格挡拼点（走格挡管道），默认 false（走伤害管道）</param>
    public static async Task ClashAll(
        CardModel card,
        IEnumerable<Creature> targets,
        decimal baseValue,
        int repeatCount,
        PlayerChoiceContext ctx,
        ValueProp clashProps = ValueProp.Move,
        Func<Creature, Task>? onVictory = null,
        Func<Creature, Task>? onDefeat = null,
        bool isBlock = false)
    {
        foreach (var target in targets)
        {
            if (!target.IsAlive) continue;
            await Clash(card, target, baseValue, repeatCount, ctx, clashProps,
                onVictory != null ? () => onVictory(target) : null,
                onDefeat != null ? () => onDefeat(target) : null,
                isBlock);
        }
    }

    /// <param name="isBlock">是否为格挡拼点（走格挡管道），默认 false（走伤害管道）</param>
    public static async Task<bool> Clash(
        CardModel card,
        Creature target,
        decimal baseValue,
        int repeatCount,
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

        // 卡牌总数值走对应管道（攻击或格挡）
        var rawCardPower = baseValue * repeatCount;
        var combatState = card.CombatState ?? playerCreature.CombatState;
        var modifiedCardPower = isBlock
            ? Hook.ModifyBlock(combatState, playerCreature,
                rawCardPower, clashProps, card, null, out _)
            : Hook.ModifyDamage(owner.RunState, combatState,
                target, playerCreature, rawCardPower, clashProps, card,
                ModifyDamageHookType.All, CardPreviewMode.None, out _);

        var enemyPower = attackIntent.GetTotalDamage(
            new[] { playerCreature }, target);

        // 双方取整后再比拼、计算差值
        var cardPowerInt = (int)decimal.Floor(modifiedCardPower);
        var enemyPowerInt = (int)decimal.Floor(enemyPower);
        bool victory = cardPowerInt >= enemyPowerInt;

        if (victory)
        {
            if (onVictory != null) await onVictory();

            var diff = cardPowerInt - enemyPowerInt;
            if (diff > 0)
                await CreatureCmd.Damage(ctx, target, diff, ValueProp.Unpowered, playerCreature, card);
        }
        else
        {
            if (onDefeat != null) await onDefeat();
        }

        return true;
    }
}
