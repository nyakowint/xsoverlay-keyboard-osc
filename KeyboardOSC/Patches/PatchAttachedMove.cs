using HarmonyLib;
using XSOverlay;

namespace KeyboardOSC.Patches;

// gosh i love enumerators (fix for a problem i created lol)
[HarmonyPatch(typeof(WindowMovementManager), "DelayedTransformSetToTarget")]
internal static class PatchAttachedMove
{
    [HarmonyPostfix]
    public static void Postfix(Unity_Overlay overlay)
    {
        var chatBar = Plugin.Instance.oscBarWindowObj.GetComponent<Unity_Overlay>();
        Plugin.Instance.RepositionBar(chatBar, overlay);
    }
}
