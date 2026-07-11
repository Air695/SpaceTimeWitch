using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Creatures;
using SpaceTimeWitch.Powers;

namespace SpaceTimeWitch.Patches;

[HarmonyPatch(typeof(Creature), nameof(Creature.TakeTurn))]
public static class Creature_TakeTurn_Patch
{
    static void Postfix(Creature __instance)
    {
        if (!__instance.IsMonster || __instance.Monster.SpawnedThisTurn)
            return;

        var combatState = __instance.CombatState;
        if (combatState == null) return;

        foreach (var player in combatState.Players)
        {
            var creature = player.Creature;
            if (creature == null || creature.IsDead) continue;

            foreach (var power in creature.Powers.OfType<IAfterMonsterAction>())
                power.OnMonsterActed();
        }
    }
}