using HarmonyLib;

namespace KeyboardOSC.Patches;

[HarmonyPatch(typeof(KeyboardInputHandler), "SendKey")]
internal static class PatchSendKey
{
    [HarmonyPrefix]
    public static bool Prefix(KeyboardKey.VirtualKeyEventData keyEventData)
    {
        if (Plugin.ChatModeActive) ChatMode.HandleKey(keyEventData);
        return true;
    }
}
