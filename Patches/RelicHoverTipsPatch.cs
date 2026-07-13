using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using SpaceTimeWitch.Cards;
using SpaceTimeWitch.Cards.KeyWords;
using SpaceTimeWitch.Extension.QG;
using SpaceTimeWitch.Scripts;

namespace SpaceTimeWitch.Patches;

[HarmonyPatch(typeof(RelicModel), "get_HoverTips")]
public static class RelicHoverTipsPatch
{
    private static readonly IReadOnlyDictionary<CardTag, string> TagToKeywordKey =
        CardTags.CustomTagToKeywordKey;

    private static readonly MethodInfo PowerGetter =
        typeof(ModelDb).GetMethod("Power", System.Array.Empty<System.Type>())!;

    static void Postfix(RelicModel __instance, ref IEnumerable<IHoverTip> __result)
    {
        var list = __result.ToList();

        if (__instance is SpaceTimeWitchRelics stwRelic)
        {
            // 自定义悬浮提示
            list.AddRange(stwRelic.ExposedCustomHoverTips);

            // QG 遗物：标签提示
            if (__instance is IQGRelic qgRelic)
            {
                foreach (var tag in qgRelic.AbnormalityTags)
                {
                    if (TagToKeywordKey.TryGetValue(tag, out var key))
                        list.Add(CustomKeyword.GetHoverTip(key));
                }
            }

            // PowerVar<T> 提示
            foreach (var v in stwRelic.ExposedCanonicalVars)
            {
                var type = v.GetType();
                if (!type.IsGenericType || type.GetGenericTypeDefinition() != typeof(PowerVar<>))
                    continue;
                var powerType = type.GetGenericArguments()[0];
                var power = PowerGetter.MakeGenericMethod(powerType).Invoke(null, null);
                list.Add(HoverTipFactory.FromPower((PowerModel)power!));
            }
        }

        __result = list;
    }
}
