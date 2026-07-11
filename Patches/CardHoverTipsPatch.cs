using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using SpaceTimeWitch.Cards;
using SpaceTimeWitch.Cards.KeyWords;

namespace SpaceTimeWitch.Patches;

[HarmonyPatch(typeof(CardModel), "get_HoverTips")]
public static class CardHoverTipsPatch
{
    private static readonly HashSet<CardTag> WatchedTags =
    [
        CardTags.Reproduce,
        CardTags.Field,
        CardTags.FSK,
        CardTags.LJSK
    ];

    private static readonly IReadOnlyDictionary<CardTag, string> TagToKeywordKey =
        CardTags.CustomTagToKeywordKey;

    private static readonly MethodInfo PowerGetter =
        typeof(ModelDb).GetMethod("Power", Array.Empty<Type>())!;

    static void Postfix(CardModel __instance, ref IEnumerable<IHoverTip> __result)
    {
        var list = __result.ToList();

        // 1. 自定义关键词标签提示
        foreach (var tag in __instance.Tags)
        {
            if (TagToKeywordKey.TryGetValue(tag, out var key))
                list.Add(CustomKeyword.GetHoverTip(key));
        }

        // 2. SpaceTimeWitch 卡牌：CardSpecificHoverTips → 自动 PowerVar<T>
        if (__instance is SpaceTimeWitchCards stwCard)
        {
            list.AddRange(stwCard.ExposedCardSpecificHoverTips);
            list.AddRange(GetPowerHoverTips(stwCard));
        }

        __result = list;
    }

    private static IEnumerable<IHoverTip> GetPowerHoverTips(SpaceTimeWitchCards card)
    {
        foreach (var v in card.ExposedCanonicalVars)
        {
            var type = v.GetType();
            if (!type.IsGenericType || type.GetGenericTypeDefinition() != typeof(PowerVar<>))
                continue;
            var powerType = type.GetGenericArguments()[0];
            var power = PowerGetter.MakeGenericMethod(powerType).Invoke(null, null);
            yield return HoverTipFactory.FromPower((PowerModel)power!);
        }
    }
}
