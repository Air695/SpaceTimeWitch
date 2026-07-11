using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Hooks;

namespace SpaceTimeWitch.Patches;

public interface IImmediateReturn { }

[HarmonyPatch(typeof(Hook), nameof(Hook.AfterCardPlayed))]
public static class ImmediateReturnPatch
{
    [HarmonyPostfix]
    static void Postfix(CardPlay cardPlay)
    {
        var card = cardPlay.Card;
        if (card is not IImmediateReturn) return;

        var player = card.Owner;
        if (player?.PlayerCombatState == null) return;
        if (player.PlayerCombatState.Hand.Cards.Count >= 10) return; // 手牌满则不回

        _ = CardPileCmd.Add(card, PileType.Hand);
    }
}