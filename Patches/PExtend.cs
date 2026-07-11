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

[HarmonyPatch(typeof(WeightedCardSelectCmd), nameof(WeightedCardSelectCmd.GenerateWeighted))]
public static class GenerateWeightedExtendPatch
{
    private class State
    {
        public int OriginalCount;
        public STWExtend? Power;
    }

    [HarmonyPrefix]
    static bool Prefix(Player player, IEnumerable<CardModel> cardPool, ref int count, out State __state)
    {
        var power = player.Creature?.GetPower<STWExtend>();
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
    static void Postfix(List<CardModel> __result, State __state)
    {
        var power = __state.Power;
        if (power == null || power.Amount <= 0) return;

        int generated = __result?.Count ?? 0;
        int withoutBonus = Math.Min(__state.OriginalCount, generated);
        int bonusUsed = generated - withoutBonus;
        if (bonusUsed <= 0) return;

        int consumed = Math.Min(bonusUsed, power.Amount);
        int remaining = power.Amount - consumed;

        if (remaining <= 0)
            _ = PowerCmd.Remove(power);
        else
            power.SetAmount(remaining);
    }
}

[HarmonyPatch(typeof(DiscoverCmd), nameof(DiscoverCmd.Discover))]
public static class DiscoverExtendPatch
{
    [HarmonyPrefix]
    static void Prefix(Player player, CardType? cardType,
        Func<CardModel, bool>? extraFilter, ref int offerCount)
    {
        var power = player.Creature?.GetPower<STWExtend>();
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

        int room = estimated - offerCount;
        int bonus = Math.Min(power.Amount, room);
        offerCount += bonus;

        int remaining = power.Amount - bonus;
        if (remaining <= 0)
            _ = PowerCmd.Remove(power);
        else
            power.SetAmount(remaining);
    }
}
