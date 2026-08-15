using HarmonyLib;
using XSOverlay;

namespace KeyboardOSC.Patches;

// Disable analytics by default (prob has minimal impact but minimize possible spam)
[HarmonyPatch(typeof(AnalyticsManager), "Initialize")]
internal static class PatchAnalytics
{
    [HarmonyPostfix]
    public static void Postfix()
    {
        XSettingsManager.Instance.Settings.SendAnalytics = false;
    }
}
