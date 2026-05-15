using HarmonyLib;
using XSOverlay;

namespace KeyboardOSC.Patches;

// Disable analytics by default — can be re-enabled after launch if desired
[HarmonyPatch(typeof(AnalyticsManager), "Initialize")]
internal static class PatchAnalytics
{
    [HarmonyPostfix]
    public static void Postfix()
    {
        XSettingsManager.Instance.Settings.SendAnalytics = false;
    }
}
