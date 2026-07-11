using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using SpaceTimeWitch.Commands;
using SpaceTimeWitch.Extension.MCJ;
using SpaceTimeWitch.Powers;
using SpaceTimeWitch.Relics;

namespace SpaceTimeWitch.Patches;

// ── WeightedCardSelectCmd ──
[HarmonyPatch(typeof(WeightedCardSelectCmd), nameof(WeightedCardSelectCmd.GenerateWeighted))]
public static class GenerateWeightedOverloadPatch
{
    private class State
    {
        public int OriginalCount;
        public ChronoOverloadPower? Power;
    }

    [HarmonyPrefix]
    static bool Prefix(Player player, IEnumerable<CardModel> cardPool, ref int count, out State __state)
    {
        var power = player.Creature?.GetPower<ChronoOverloadPower>();
        __state = new State { OriginalCount = count, Power = power };

        if (power == null || power.Amount <= 0) return true;

        if (cardPool.Count() < count)
        {
            __state.Power = null;
            return true;
        }

        count += power.Amount;
        return true;
    }

    [HarmonyPostfix]
    static void Postfix(List<CardModel> __result, Player player, State __state)
    {
        if (__state.Power == null) return;
        if (!ReproduceContext.ActivePlayers.Contains(player)) return;

        foreach (var card in __result ?? Enumerable.Empty<CardModel>())
            card.AddKeyword(CardKeyword.Ethereal);
    }
}

// ── DiscoverCmd（仅改 offerCount）──
[HarmonyPatch(typeof(DiscoverCmd), nameof(DiscoverCmd.Discover))]
public static class DiscoverOverloadPatch
{
    [HarmonyPrefix]
    static void Prefix(Player player, CardType? cardType,
        Func<CardModel, bool>? extraFilter, ref int offerCount)
    {
        var power = player.Creature?.GetPower<ChronoOverloadPower>();
        if (power == null || power.Amount <= 0) return;

        var activeTags = player.Relics
            .OfType<ITagRelic>()
            .Select(r => r.AssociatedTag)
            .ToHashSet();

        if (activeTags.Count == 0) return;

        var pickaxeTypeNames = player.Relics
            .OfType<IPickaxeRelic>()
            .SelectMany(p =>
            {
                var bindings = PickaxeCardBindings.Deserialize(p.GetCardBindingsData());
                bool shieldDisabled = ShieldCraftSubMenu.IsSlotDisabled(p);
                return bindings.AllSlots
                    .Where((t, i) => t != null && !(i == 5 && shieldDisabled))
                    .Select(t => t!.Name);
            })
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        int estimated = ModelDb.AllCardPools
            .SelectMany(p => p.GetUnlockedCards(
                player.UnlockState,
                player.RunState.CardMultiplayerConstraint))
            .Where(c => c.CanBeGeneratedInCombat
                        && (c.Tags.Any(t => activeTags.Contains(t))
                            || pickaxeTypeNames.Contains(c.GetType().Name))
                        && (cardType == null || c.Type == cardType)
                        && (extraFilter == null || extraFilter(c)))
            .DistinctBy(c => c.Id)
            .Count();

        if (estimated < offerCount) return;

        offerCount += power.Amount;
    }
}

// ── CardSelectCmd.FromSimpleGrid：选前给选项加虚无 ──
[HarmonyPatch(typeof(CardSelectCmd), nameof(CardSelectCmd.FromSimpleGrid))]
public static class FromSimpleGridOverloadPatch
{
    [HarmonyPrefix]
    static void Prefix(IReadOnlyList<CardModel> cardsIn, Player player)
    {
        if (!ReproduceContext.ActivePlayers.Contains(player)) return;

        var power = player.Creature?.GetPower<ChronoOverloadPower>();
        if (power == null || power.Amount <= 0) return;

        foreach (var card in cardsIn)
            card.AddKeyword(CardKeyword.Ethereal);
    }
}
