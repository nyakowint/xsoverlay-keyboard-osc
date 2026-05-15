using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using WindowsInput;
using WindowsInput.Native;

namespace KeyboardOSC.Patches;

// Blocks key inputs from being sent to Windows while chat mode is active.
// Modifier/media/IME keys are passed through so XSO's own logic keeps working.
[HarmonyPatch]
internal static class PatchBlockInput
{
    private static readonly System.Type[] ParamTypes = [typeof(VirtualKeyCode)];

    // TargetMethods cannot return zero results — Prepare skips the patch entirely if none are found
    static bool Prepare()
    {
        return AccessTools.Method(typeof(KeyboardSimulator), nameof(KeyboardSimulator.KeyPress), ParamTypes) != null
            || AccessTools.Method(typeof(KeyboardSimulator), nameof(KeyboardSimulator.KeyDown), ParamTypes) != null
            || AccessTools.Method(typeof(KeyboardSimulator), nameof(KeyboardSimulator.KeyUp), ParamTypes) != null;
    }

    static IEnumerable<MethodBase> TargetMethods()
    {
        var methods = new[]
        {
            AccessTools.Method(typeof(KeyboardSimulator), nameof(KeyboardSimulator.KeyPress), ParamTypes),
            AccessTools.Method(typeof(KeyboardSimulator), nameof(KeyboardSimulator.KeyDown), ParamTypes),
            AccessTools.Method(typeof(KeyboardSimulator), nameof(KeyboardSimulator.KeyUp), ParamTypes),
        };
        foreach (var m in methods)
            if (m != null) yield return m;
    }

    [HarmonyPrefix]
    public static bool Prefix(VirtualKeyCode keyCode)
    {
        if (!Plugin.ChatModeActive) return true;

        var passthroughKeys = new List<VirtualKeyCode>
        {
            // Modifier keys
            VirtualKeyCode.LSHIFT, VirtualKeyCode.RSHIFT,
            VirtualKeyCode.LCONTROL, VirtualKeyCode.RCONTROL,
            VirtualKeyCode.LALT, VirtualKeyCode.RALT,
            VirtualKeyCode.CAPITAL, VirtualKeyCode.PRNTSCRN,

            // Windows/media keys so wrist overlay still works
            VirtualKeyCode.LWIN, VirtualKeyCode.RWIN,
            VirtualKeyCode.PLAY, VirtualKeyCode.PAUSE,
            VirtualKeyCode.MEDIA_PLAY_PAUSE, VirtualKeyCode.MEDIA_STOP,
            VirtualKeyCode.MEDIA_NEXT_TRACK, VirtualKeyCode.MEDIA_PREV_TRACK,
            VirtualKeyCode.UP, VirtualKeyCode.DOWN,
            VirtualKeyCode.LEFT, VirtualKeyCode.RIGHT,

            // IME keys
            VirtualKeyCode.KANA, VirtualKeyCode.KANJI, VirtualKeyCode.HANGUL,
            VirtualKeyCode.HANGEUL, VirtualKeyCode.HANJA, VirtualKeyCode.FINAL,
            VirtualKeyCode.JUNJA, VirtualKeyCode.CONVERT, VirtualKeyCode.NONCONVERT,
            VirtualKeyCode.MODECHANGE, VirtualKeyCode.ACCEPT,
            VirtualKeyCode.PROCESSKEY,
        };
        return passthroughKeys.Contains(keyCode);
    }
}
