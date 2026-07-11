using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using SpaceTimeWitch.Commands;
using SpaceTimeWitch.Powers;

namespace SpaceTimeWitch.Patches;

// ── 标记：当前是否在复现流程中 ──
internal static class ReproduceContext
{
    public static readonly HashSet<Player> ActivePlayers = new();
}

// ── DiscoverCmd ──
[HarmonyPatch(typeof(DiscoverCmd), nameof(DiscoverCmd.Discover))]
public static class DiscoverCapturePatch
{
    [HarmonyPrefix]
    static void Prefix(Player player)
    {
        ReproduceContext.ActivePlayers.Add(player);
    }
}

// ── WeightedCardSelectCmd.PickFromCards ──
[HarmonyPatch(typeof(WeightedCardSelectCmd), nameof(WeightedCardSelectCmd.PickFromCards))]
public static class PickFromCardsCapturePatch
{
    [HarmonyPrefix]
    static void Prefix(Player player)
    {
        ReproduceContext.ActivePlayers.Add(player);
    }
}

// ── WeightedCardSelectCmd.PickFromPools ──
[HarmonyPatch(typeof(WeightedCardSelectCmd), nameof(WeightedCardSelectCmd.PickFromPools))]
public static class PickFromPoolsCapturePatch
{
    [HarmonyPrefix]
    static void Prefix(Player player)
    {
        ReproduceContext.ActivePlayers.Add(player);
    }
}

// ── CardSelectCmd.FromSimpleGrid：存入未选择的卡 + 清标记 ──
[HarmonyPatch(typeof(CardSelectCmd), nameof(CardSelectCmd.FromSimpleGrid))]
public static class FromSimpleGridCapturePatch
{
    [HarmonyPostfix]
    static async Task<IEnumerable<CardModel>> Postfix(Task<IEnumerable<CardModel>> __result,
        IReadOnlyList<CardModel> cardsIn, Player player)
    {
        if (!ReproduceContext.ActivePlayers.Remove(player))
            return await __result;

        var archPower = player.Creature?.GetPower<ChronoCapturePower>();
        if (archPower == null || archPower.Amount <= 0)
            return await __result;

        var cards = await __result;
        var selectedSet = cards.ToHashSet();
        var unselected = cardsIn.Where(c => !selectedSet.Contains(c));

        foreach (var card in unselected)
            _ = PersonalSpaceCmd.Store(player, card);

        return cards;
    }
}
