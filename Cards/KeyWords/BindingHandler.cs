using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using SpaceTimeWitch.Powers;
using STS2RitsuLib.Keywords;

namespace SpaceTimeWitch.Cards.KeyWords;

/// <summary>
/// Harmony Prefix on AfterCardPlayed — 将本回合打出的 Binding 卡牌加入静态列表，
/// BindingReturnPower 在回合开始时从列表读取并归还手牌。
/// </summary>
[HarmonyPatch(typeof(Hook), nameof(Hook.AfterCardPlayed))]
public static class BindingHandler
{
    private static readonly HashSet<CardModel> s_playedThisTurn = new();

    [HarmonyPrefix]
    static void Prefix(CardPlay cardPlay)
    {
        var card = cardPlay.Card;
        if (card.Owner?.Creature == null) return;
        if (!card.HasModKeyword(STWKeywords.Binding)) return;

        s_playedThisTurn.Add(card);

        // 确保 BindingReturnPower 存在（fire-and-forget）
        _ = EnsurePower(card);
    }

    private static async Task EnsurePower(CardModel card)
    {
        var creature = card.Owner?.Creature;
        if (creature == null) return;
        if (creature.Powers.OfType<BindingReturnPower>().Any()) return;
        await PowerCmd.Apply<BindingReturnPower>(new ThrowingPlayerChoiceContext(), creature, 1, creature, card);
    }

    /// <summary>供 BindingReturnPower 读取并清空本回合追踪的卡牌</summary>
    internal static HashSet<CardModel> ConsumePlayedCards()
    {
        var result = new HashSet<CardModel>(s_playedThisTurn);
        s_playedThisTurn.Clear();
        return result;
    }
}
