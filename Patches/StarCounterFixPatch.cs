using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace SpaceTimeWitch.Patches;

/// <summary>
/// RitsuLib 的 CharacterEnergyCounterRuntimeFactoryPatch 包装了模组能量球场景，
/// 导致 reparent 后星计数器的父节点类型与预期不同（可能是 NParticlesContainer 而非 Control）。
/// 此 Patch 在 NCombatUi.Activate 完成后直接修正星计数器的锚点和偏移。
/// </summary>
[HarmonyPatch(typeof(NCombatUi), nameof(NCombatUi.Activate))]
public static class StarCounterFixPatch
{
    static void Postfix(NCombatUi __instance)
    {
        var starField = typeof(NCombatUi).GetField("_starCounter",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (starField == null) return;

        var starCounter = starField.GetValue(__instance) as NStarCounter;
        if (starCounter == null) return;

        // 恢复星计数器的默认锚点和偏移（和 star_counter.tscn 一致）
        starCounter.AnchorTop = 1.0f;
        starCounter.AnchorBottom = 1.0f;
        starCounter.OffsetLeft = 64f;
        starCounter.OffsetTop = -212f;
        starCounter.OffsetRight = 192f;
        starCounter.OffsetBottom = -84f;
        starCounter.Scale = new Godot.Vector2(0.8f, 0.8f);
    }
}