using HarmonyLib;
using Vuplex.WebView;
using XSOverlay;

namespace KeyboardOSC.Patches;

/// <summary>
/// XSOverlay drops webview keyboard focus when enter or escape is pressed, which would kick the
/// user out of the chat bar after every message. While chat mode is on we forward those keys into
/// the webview instead and let the chat bar decide what they mean.
/// </summary>
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
                // Usually the chat bar asking for focus again (the page re-requests it whenever
                // the input regains focus), so leave chat mode alone and let the webview decide.
                return true;
            default:
                // Focus is going away for a reason we don't control (keyboard closed, another
                // input took over, ...) so chat mode is over whether the webview told us or not.
                Plugin.ChatModeActive = false;
                return true;
        }
    }
}
