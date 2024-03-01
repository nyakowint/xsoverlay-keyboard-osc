using System.Collections.Generic;
using System.Threading.Tasks;
using HarmonyLib;
using KeyboardOSC.Twitch;
using Steamworks;
using UnityEngine;
using WindowsInput;
using WindowsInput.Native;
using XSOverlay;
using XSOverlay.WebApp;
using XSOverlay.Websockets.API;

namespace KeyboardOSC;

// ReSharper disable InconsistentNaming
public static class Patches
{
    private static Harmony Harmony;

    private static bool HasSettingsBeenOpenedOnce;

    public static void PatchAll()
    {
        Harmony = new Harmony("nwnt.keyboardosc");

        #region Patch Key Presses

        var sendKeyMethod = AccessTools.Method(typeof(KeyboardInputHandler), "SendKey");
        var sendKeyPatch = new HarmonyMethod(typeof(Patches).GetMethod(nameof(KeyboardPatch)));
        Harmony.Patch(sendKeyMethod, sendKeyPatch);

        #endregion

        #region Stop inputs being sent to overlays

        // stop inputs being sent to overlays
        var keyPress = AccessTools.Method(typeof(KeyboardSimulator), nameof(KeyboardSimulator.KeyPress),
            new[] { typeof(VirtualKeyCode) });
        var keyDown = AccessTools.Method(typeof(KeyboardSimulator), nameof(KeyboardSimulator.KeyDown),
            new[] { typeof(VirtualKeyCode) });
        var keyUp = AccessTools.Method(typeof(KeyboardSimulator), nameof(KeyboardSimulator.KeyUp),
            new[] { typeof(VirtualKeyCode) });
        var blockInputPatch = new HarmonyMethod(typeof(Patches).GetMethod(nameof(BlockInput)));

        Harmony.Patch(keyPress, prefix: blockInputPatch);
        Harmony.Patch(keyDown, prefix: blockInputPatch);
        Harmony.Patch(keyUp, prefix: blockInputPatch);

        #endregion


        // Scale bar with keyboard
        var scaleMethod =
            AccessTools.Method(typeof(WindowMovementManager), nameof(WindowMovementManager.DoScaleWindowFixed));
        var scalePatch = new HarmonyMethod(typeof(Patches).GetMethod(nameof(ScalePatch)));
        Harmony.Patch(scaleMethod, null, scalePatch);

        // gosh i love enumerators (fix for a problem i created lol)
        var attachMethod = AccessTools.Method(typeof(WindowMovementManager), "DelayedTransformSetToTarget");
        var attachPatch = new HarmonyMethod(typeof(Patches).GetMethod(nameof(AttachedMovePatch)));
        Harmony.Patch(attachMethod, null, attachPatch);

        // Disable analytics by default (Xiexe loves seeing my plugin errors im sure XD)
        // can be turned back on after launching if you want to send him stuff for some reason
        var initAnalytics = AccessTools.Method(typeof(AnalyticsManager), "Initialize");
        var analyticsPatch = new HarmonyMethod(typeof(Patches).GetMethod(nameof(AnalyticsPatch)));
        Harmony.Patch(initAnalytics, postfix: analyticsPatch);


        #region Settings UI Related Patches

        var toggleAppSettings =
            AccessTools.Method(typeof(Overlay_Manager), nameof(Overlay_Manager.ToggleEditMode));
        var settingsOverlayPatch = new HarmonyMethod(typeof(Patches).GetMethod(nameof(SettingsOverlayPatch)));

        var setSettings = AccessTools.Method(typeof(XSettingsManager), nameof(XSettingsManager.SetSetting));
        var setSettingsPatch = new HarmonyMethod(typeof(Patches).GetMethod(nameof(SetSettingPatch)));

        var reqSettings = AccessTools.Method(typeof(ApiHandler), "OnRequestCurrentSettings");
        var reqSettingsPatch = new HarmonyMethod(typeof(Patches).GetMethod(nameof(RequestSettingsPatch)));

        Harmony.Patch(toggleAppSettings, postfix: settingsOverlayPatch);
        Harmony.Patch(setSettings, setSettingsPatch);
        Harmony.Patch(reqSettings, reqSettingsPatch);

        #endregion
    }

    public static bool KeyboardPatch(KeyboardKey.VirtualKeyEventData keyEventData)
    {
        if (Plugin.ChatModeActive) ChatMode.HandleKey(keyEventData);
        return true;
    }

    public static void AnalyticsPatch()
    {
        XSettingsManager.Instance.Settings.SendAnalytics = false;
    }

    public static void AttachedMovePatch(Unity_Overlay overlay, Transform target, Overlay_TrackDevice deviceTracker,
        Unity_Overlay.OverlayTrackedDevice device, Vector3 pos, Quaternion rot, float originalScale,
        float originalOpacity)
    {
        var chatBar = Plugin.Instance.oscBarWindowObj.GetComponent<Unity_Overlay>();
        Plugin.Instance.RepositionBar(chatBar, overlay);
    }

    public static void ScalePatch(float dist, Unity_Overlay activeOverlay, float StartingWidth)
    {
        if (!Plugin.ChatModeActive || activeOverlay.overlayKey != "xso.overlay.keyboard") return;
        var chatBar = Plugin.Instance.oscBarWindowObj.GetComponent<Unity_Overlay>();
        chatBar.opacity = activeOverlay.opacity;
        Plugin.Instance.RepositionBar(chatBar, activeOverlay);
    }

    public static void SettingsOverlayPatch()
    {
        if (!Plugin.ModifiedUiSuccess || HasSettingsBeenOpenedOnce) return;
        Plugin.PluginLogger.LogInfo("Replacing settings page url!");
        var globalSettings = Overlay_Manager.Instance.GlobalSettingsMenuOverlay;
        var loadString =
            $"http://localhost:{ExternalMessageHandler.Instance.Config.WebSocketPort + 1}/apps/_UI/Default/Settings/SettingsKO.html";
        globalSettings.OverlayWebView._webView.WebView.LoadUrl(loadString);
        HasSettingsBeenOpenedOnce = true;
    }

    public static void RequestSettingsPatch(string sender, string data)
    {
        // create new UiSettings instance
        var pluginVersion = Plugin.AssemblyVersion;
        if (SteamClient.IsValid && SteamApps.CurrentBetaName != null)
        {
            pluginVersion += $" — you're on {SteamApps.CurrentBetaName} branch, check repo for beta plugin releases";
        }
        
        var settings = new UiSettings
        {
            KBCheckForUpdates = PluginSettings.GetSetting<bool>("CheckForUpdates").Value,
            KBLiveSend = PluginSettings.GetSetting<bool>("LiveSend").Value,
            KBTypingIndicator = PluginSettings.GetSetting<bool>("TypingIndicator").Value,
            KBTwitchSending = PluginSettings.GetSetting<bool>("TwitchSending").Value,
            KBVersion = pluginVersion,
        };
        var data2 = JsonUtility.ToJson(settings, false);
        ServerClientBridge.Instance.Api.SendMessage("UpdateSettings", data2, null, sender);
    }

    public static bool SetSettingPatch(string name, string value, string value1, bool sendAnalytics = false)
    {
        switch (name)
        {
            case "GoToOgSettings":
                var loadString =
                    $"http://localhost:{ExternalMessageHandler.Instance.Config.WebSocketPort + 1}/ui/Settings.html";
                Overlay_Manager.Instance.GlobalSettingsMenuOverlay.OverlayWebView._webView.WebView.LoadUrl(loadString);
                Overlay_Manager.Instance.ToggleApplicationSettings();
                break;
            case "KBCheckForUpdates":
                PluginSettings.SetSetting<bool>("CheckForUpdates", value);
                break;
            case "KBLiveSend":
                PluginSettings.SetSetting<bool>("LiveSend", value);
                break;
            case "KBTypingIndicator":
                PluginSettings.SetSetting<bool>("TypingIndicator", value);
                break;
            case "KBTwitchSending":
                PluginSettings.SetSetting<bool>("TwitchSending", value);
                break;
            case "KBOpenRepo":
                Application.OpenURL("https://github.com/nyakowint/xsoverlay-keyboard-osc");
                Tools.SendBread("KeyboardOSC Github link opened in browser!");
                break;
            case "KBVersionCheck":
                Task.Run(Tools.CheckVersion);
                break;
        }
        
        if (name.StartsWith("Twitch"))
            Core.SettingsCallback(name, value);

        return true;
    }

    public static bool BlockInput(VirtualKeyCode keyCode)
    {
        if (!Plugin.ChatModeActive) return true;

        // small caveat with the way i'm doing this:
        // modifier keys still get passed to windows so that i don't have to reimplement xso's logic for them
        var passthroughKeys = new List<VirtualKeyCode>
        {
            // Modifier keys
            VirtualKeyCode.LSHIFT, VirtualKeyCode.RSHIFT,
            VirtualKeyCode.LCONTROL, VirtualKeyCode.RCONTROL,
            VirtualKeyCode.LALT, VirtualKeyCode.RALT,
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