using HarmonyLib;
using XSOverlay;

namespace KeyboardOSC.Patches;

[HarmonyPatch(typeof(WindowMovementManager), nameof(WindowMovementManager.DoScaleWindowFixed))]
internal static class PatchScaleWindow
{
    [HarmonyPostfix]
    public static void Postfix(Unity_Overlay activeOverlay)
    {
        if (!Plugin.ChatModeActive || activeOverlay.overlayKey != "xso.overlay.keyboard") return;
        var chatBar = Plugin.Instance.oscBarWindowObj.GetComponent<Unity_Overlay>();
        chatBar.opacity = activeOverlay.opacity;
        Plugin.Instance.RepositionBar(chatBar, activeOverlay);
    }
}
