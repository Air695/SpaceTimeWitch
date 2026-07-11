using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using SpaceTimeWitch.Nodes;

namespace SpaceTimeWitch.Patches;

/// <summary>
/// 在以下时机刷新标签遗物卡池查看牌堆：
/// 1. 战斗 UI 初始化时
/// 2. 获得遗物时
/// 3. 遗物被替换（升级）时
/// </summary>
[HarmonyPatch]
public static class TagRelicPoolPatch
{
    [HarmonyPatch(typeof(NCombatPilesContainer), nameof(NCombatPilesContainer.Initialize))]
    [HarmonyPostfix]
    private static void OnCombatStart(Player player)
    {
        TagRelicPoolManager.RefreshPile(player);
    }

    [HarmonyPatch(typeof(RelicCmd), nameof(RelicCmd.Obtain), typeof(RelicModel), typeof(Player), typeof(int))]
    [HarmonyPostfix]
    private static void OnRelicObtained(RelicModel relic, Player player)
    {
        TagRelicPoolManager.RefreshPile(player);
    }

    [HarmonyPatch(typeof(RelicCmd), nameof(RelicCmd.Replace))]
    [HarmonyPostfix]
    private static void OnRelicReplaced(RelicModel original)
    {
        if (original.Owner != null)
            TagRelicPoolManager.RefreshPile(original.Owner);
    }
}
