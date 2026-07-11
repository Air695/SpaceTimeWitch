using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.RestSite;
using MegaCrit.Sts2.Core.Localization;
using SpaceTimeWitch.Extension.MCJ;

namespace SpaceTimeWitch.Patches;

[HarmonyPatch(typeof(RestSiteOption), "get_Icon")]
public static class RestSiteIconPatch
{
    static bool Prefix(RestSiteOption __instance, ref Texture2D? __result)
    {
        if (__instance is ICustomRestSiteIcon custom && !string.IsNullOrEmpty(custom.CustomIconPath))
        {
            __result = GD.Load<Texture2D>(custom.CustomIconPath);
            return false;
        }
        return true;
    }
}

[HarmonyPatch(typeof(RestSiteOption), "get_Title")]
public static class RestSiteTitlePatch
{
    static void Postfix(RestSiteOption __instance, ref LocString __result)
    {
        if (__instance is ICustomRestSiteIcon custom)
            __result = custom.CustomTitle;
    }
}

[HarmonyPatch(typeof(RestSiteOption), "get_IsEnabled")]
public static class RestSiteIsEnabledPatch
{
    static void Postfix(RestSiteOption __instance, ref bool __result)
    {
        if (__instance is ICustomRestSiteIcon custom)
            __result = custom.CustomIsEnabled;
    }
}
