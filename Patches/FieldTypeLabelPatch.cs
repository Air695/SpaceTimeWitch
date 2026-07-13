using Godot;
using HarmonyLib;
using MegaCrit.Sts2.addons.mega_text;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.Cards;
using SpaceTimeWitch.Cards;

namespace SpaceTimeWitch.Patches;

/// <summary>
/// 将场地卡的卡牌类型标签从"能力"替换为本地化字符，并同步稀有度铭牌颜色。
/// CardType.None 会导致原方法抛异常，因此用 Prefix 拦截后跳过原方法。
/// 本地化 key: cards.json 中的 "FIELD"
/// </summary>
[HarmonyPatch(typeof(NCard), "UpdateTypePlaque")]
public static class FieldTypeLabelPatch
{
    static bool Prefix(NCard __instance)
    {
        if (!__instance.Model.Tags.Contains(CardTags.Field))
            return true; // 不是场地卡，走原方法

        var label = __instance.GetNode<MegaLabel>("%TypeLabel");
        label.SetTextAutoSize(new LocString("cards", "FIELD").GetFormattedText());

        // 同步稀有度对应的铭牌材质（颜色跟随稀有度）
        var plaque = __instance.GetNode<NinePatchRect>("%TypePlaque");
        var bannerMaterial = __instance.Model.BannerMaterial;
        if (plaque.Material != bannerMaterial)
            plaque.Material = bannerMaterial;

        return false;    // 跳过原方法，避免 CardType.None 抛异常
    }
}
