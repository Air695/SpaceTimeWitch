using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Hooks;
using STS2RitsuLib.Keywords;

namespace SpaceTimeWitch.Cards.KeyWords;

/// <summary>
/// Soulbinding（灵魂绑定）：打出后返回手牌。
/// </summary>
[HarmonyPatch(typeof(Hook), nameof(Hook.AfterCardPlayed))]
public static class SoulbindingHandler
{
    [HarmonyPostfix]
    static async void Postfix(CardPlay cardPlay)
    {
        var card = cardPlay.Card;
        if (!card.HasModKeyword(STWKeywords.Soulbinding)) return;
        if (card.Pile?.Type == PileType.Exhaust) return;
        if (card.Owner == null) return;

        await CardPileCmd.Add(card, PileType.Hand);
    }
}
