using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Models;

namespace SpaceTimeWitch.Field;

/// <summary>
/// 拦截 PowerCmd.Remove：阻止外部效果（buff 清除、死亡清理等）移除场地能力。
/// 仅在主动替换场地时允许移除。
/// </summary>
[HarmonyPatch(typeof(PowerCmd), "Remove", typeof(PowerModel))]
public static class PowerCmd_Remove_Patch
{
    static bool Prefix(PowerModel? power)
    {
        if (power is FieldPowerBase && !FieldCmd.IsReplacing)
        {
            // 非主动替换：阻止移除（跳过原始方法）
            return false;
        }
        return true;
    }
}
