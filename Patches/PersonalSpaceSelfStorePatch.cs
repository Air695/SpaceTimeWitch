using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Nodes.Cards;
using SpaceTimeWitch.Commands;

namespace SpaceTimeWitch.Patches;

public interface IPersonalSpaceSelfStore { }

/// <summary>
/// 实现 IPersonalSpaceSelfStore 的卡牌打出后自动存入个人空间。
/// </summary>
[HarmonyPatch(typeof(Hook), nameof(Hook.AfterCardPlayed))]
public static class PersonalSpaceSelfStorePatch
{
    [HarmonyPostfix]
    static async Task Postfix(Task __result, CardPlay cardPlay)
    {
        await __result;

        var card = cardPlay.Card;
        if (card is not IPersonalSpaceSelfStore) return;

        var player = card.Owner;
        if (player?.PlayerCombatState == null) return;

        await PersonalSpaceCmd.Store(player, card);

        // 清理残留节点
        var cardNode = NCard.FindOnTable(card);
        if (cardNode != null)
        {
            cardNode.Visible = false;
            cardNode.GetParent()?.RemoveChild(cardNode);
            cardNode.QueueFree();
        }
    }
}
