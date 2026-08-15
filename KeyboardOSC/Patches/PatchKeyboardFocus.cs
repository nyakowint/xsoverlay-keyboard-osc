using HarmonyLib;
using Vuplex.WebView;
using XSOverlay;

namespace KeyboardOSC.Patches;

[HarmonyPatch(typeof(KeyboardInputFocusManager), nameof(KeyboardInputFocusManager.Release))]
internal static class PatchKeyboardFocus
{
    [HarmonyPrefix]
    public static bool Prefix(KeyboardInputFocusManager __instance, string reason)
    {
        if (!Plugin.ChatModeActive || !__instance.HasFocus) return true;

        switch (reason)
        {
            case "enter":
                __instance.SendKey("Enter", KeyModifier.None);
                return false;
            case "escape":
                __instance.SendKey("Escape", KeyModifier.None);
                return false;
            case "replaced":
                return true;
            default:
                Plugin.ChatModeActive = false;
                return true;
        }
    }
}
