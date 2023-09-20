﻿using System.Collections.Generic;
using BepInEx.Logging;
using HarmonyLib;
using WindowsInput;
using WindowsInput.Native;
using XSOverlay;

// ReSharper disable SwitchStatementHandlesSomeKnownEnumValuesWithDefault

namespace KeyboardOSC;

public static class Patches
{
    private static readonly ManualLogSource Logger = Plugin.PluginLogger;
    public static Harmony Harmony;

    public static void DoPatches()
    {
        Harmony = new Harmony("nwnt.keyboardosc");
        // patch key presses
        var sendKeyMethod = AccessTools.Method(typeof(KeyboardInputHandler), "SendKey");
        var sendKeyPatch = new HarmonyMethod(typeof(Patches).GetMethod(nameof(KeyboardPatch)));
        Harmony.Patch(sendKeyMethod, sendKeyPatch);

        // scale bar with keyboard?
        var scaleMethod =
            AccessTools.Method(typeof(WindowMovementManager), nameof(WindowMovementManager.DoScaleWindowFixed));
        var scalePatch = new HarmonyMethod(typeof(Patches).GetMethod(nameof(ScalePatch)));
        Harmony.Patch(scaleMethod, null, scalePatch);

        // stop inputs being sent to overlays
        var keyPress = AccessTools.Method(typeof(KeyboardSimulator), nameof(KeyboardSimulator.KeyPress),
            new[] {typeof(VirtualKeyCode)});
        var keyDown = AccessTools.Method(typeof(KeyboardSimulator), nameof(KeyboardSimulator.KeyDown),
            new[] {typeof(VirtualKeyCode)});
        var keyUp = AccessTools.Method(typeof(KeyboardSimulator), nameof(KeyboardSimulator.KeyUp),
            new[] {typeof(VirtualKeyCode)});
        var blockInputPatch = new HarmonyMethod(typeof(Patches).GetMethod(nameof(BlockInput)));
        Harmony.Patch(keyPress, prefix: blockInputPatch);
        Harmony.Patch(keyDown, prefix: blockInputPatch);
        Harmony.Patch(keyUp, prefix: blockInputPatch);
        
        // Disable analytics by default (Xiexe loves seeing plugin errors im sure XD)
        var initAnalytics = AccessTools.Method(typeof(AnalyticsManager), "Initialize");
        var analyticsPatch = new HarmonyMethod(typeof(Patches).GetMethod(nameof(AnalyticsPatch)));
        Harmony.Patch(initAnalytics, postfix: analyticsPatch);
    }

    public static bool KeyboardPatch(KeyboardKey.VirtualKeyEventData keyEventData)
    {
        Logger.LogWarning($"patch: {keyEventData.Sender.Label}");
        Logger.LogWarning($"{keyEventData.Sender.SecondaryLabel}");
        Logger.LogWarning($"{keyEventData.IsKeySticky}");
        if (Plugin.IsChatModeActive) ChatModeManager.HandleKey(keyEventData);
        return true;
    }

    public static void AnalyticsPatch()
    {
        XSettingsManager.Instance.Settings.SendAnalytics = false;
    }
    
    public static void ScalePatch(float dist, Unity_Overlay activeOverlay, float StartingWidth)
    {
        if (!Plugin.IsChatModeActive || activeOverlay.overlayKey != "xso.overlay.keyboard") return;
        var barOverlay = Plugin.Instance.oscBarWindowObj.GetComponent<Unity_Overlay>();
        barOverlay.widthInMeters = activeOverlay.widthInMeters - 0.01f;
    }

    public static bool BlockInput(VirtualKeyCode keyCode)
    {
        if (!Plugin.IsChatModeActive) return true;
        
        // small caveat with the way i'm doing this:
        // modifier keys still get passed to windows so that i don't have to reimplement xso's logic for them
        var passthroughKeys = new List<VirtualKeyCode>
        {
            // Modifier keys
            VirtualKeyCode.LSHIFT, VirtualKeyCode.RSHIFT,
            VirtualKeyCode.LCONTROL, VirtualKeyCode.RCONTROL,
            VirtualKeyCode.LALT, VirtualKeyCode.RALT,
            // printscreen key cause screenshots sharex ftw
            VirtualKeyCode.CAPITAL, VirtualKeyCode.PRNTSCRN, // aka VirtualKeyCode.SNAPSHOT

            // windows keys and media keys so the wrist one still functions+
            VirtualKeyCode.LWIN, VirtualKeyCode.RWIN,
            VirtualKeyCode.PLAY, VirtualKeyCode.PAUSE, // no idea if anything uses these buuut just in case
            VirtualKeyCode.MEDIA_PLAY_PAUSE, VirtualKeyCode.MEDIA_STOP,
            VirtualKeyCode.MEDIA_NEXT_TRACK, VirtualKeyCode.MEDIA_PREV_TRACK,
            // and finally arrow keys if needed
            VirtualKeyCode.UP, VirtualKeyCode.DOWN,
            VirtualKeyCode.LEFT, VirtualKeyCode.RIGHT
        };
        return passthroughKeys.Contains(keyCode);
    }
}