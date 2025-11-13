using System.Collections.Generic;
using System.Threading.Tasks;
using HarmonyLib;
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

        var sendKeyMethod = Tools.SafeMethod(typeof(KeyboardInputHandler), "SendKey", required: true);
        var sendKeyPatch = new HarmonyMethod(typeof(Patches).GetMethod(nameof(KeyboardPatch)));
        if (sendKeyMethod != null) Harmony.Patch(sendKeyMethod, sendKeyPatch);

        #endregion

        #region Stop inputs being sent to overlays

        // stop inputs being sent to overlays
        var keyPress = Tools.SafeMethod(typeof(KeyboardSimulator), nameof(KeyboardSimulator.KeyPress),
            [typeof(VirtualKeyCode)]);
        var keyDown = Tools.SafeMethod(typeof(KeyboardSimulator), nameof(KeyboardSimulator.KeyDown),
            [typeof(VirtualKeyCode)]);
        var keyUp = Tools.SafeMethod(typeof(KeyboardSimulator), nameof(KeyboardSimulator.KeyUp),
            [typeof(VirtualKeyCode)]);
        var blockInputPatch = new HarmonyMethod(typeof(Patches).GetMethod(nameof(BlockInputPatch)));

        if (keyPress != null) Harmony.Patch(keyPress, prefix: blockInputPatch);
        if (keyDown != null) Harmony.Patch(keyDown, prefix: blockInputPatch);
        if (keyUp != null) Harmony.Patch(keyUp, prefix: blockInputPatch);

        #endregion


        // Scale bar with keyboard
        var scaleMethod =
            Tools.SafeMethod(typeof(WindowMovementManager), nameof(WindowMovementManager.DoScaleWindowFixed));
        var scalePatch = new HarmonyMethod(typeof(Patches).GetMethod(nameof(ScalePatch)));
        if (scaleMethod != null) Harmony.Patch(scaleMethod, null, scalePatch);

        // gosh i love enumerators (fix for a problem i created lol)
        var attachMethod = Tools.SafeMethod(typeof(WindowMovementManager), "DelayedTransformSetToTarget");
        var attachPatch = new HarmonyMethod(typeof(Patches).GetMethod(nameof(AttachedMovePatch)));
        if (attachMethod != null) Harmony.Patch(attachMethod, null, attachPatch);

        // Disable analytics by default (Xiexe loves seeing my plugin errors im sure XD)
        // can be turned back on after launching if you want to send him stuff for some reason
        var initAnalytics = Tools.SafeMethod(typeof(AnalyticsManager), "Initialize");
        var analyticsPatch = new HarmonyMethod(typeof(Patches).GetMethod(nameof(AnalyticsPatch)));
        if (initAnalytics != null) Harmony.Patch(initAnalytics, postfix: analyticsPatch);


        #region Settings UI Related Patches

        var toggleAppSettings =
            Tools.SafeMethod(typeof(Overlay_Manager), nameof(Overlay_Manager.ToggleEditMode));
        var settingsOverlayPatch = new HarmonyMethod(typeof(Patches).GetMethod(nameof(SettingsOverlayPatch)));


        var setSettings = Tools.SafeMethod(typeof(XSettingsManager), nameof(XSettingsManager.SetSetting));
        var setSettingsPatch = new HarmonyMethod(typeof(Patches).GetMethod(nameof(SetSettingPatch)));

        var reqSettings = Tools.SafeMethod(typeof(ApiHandler), "OnRequestCurrentSettings");
        var reqSettingsPatch = new HarmonyMethod(typeof(Patches).GetMethod(nameof(RequestSettingsPatch)));

        Harmony.Patch(toggleAppSettings, postfix: settingsOverlayPatch);
        if (setSettings != null) Harmony.Patch(setSettings, setSettingsPatch);
        if (reqSettings != null) Harmony.Patch(reqSettings, reqSettingsPatch);

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
        Plugin.PluginLogger.LogInfo("[PATCHES] Replacing settings page url!");
        var globalSettings = Overlay_Manager.Instance.GlobalSettingsMenuOverlay;
        var loadString =
            $"http://localhost:{ExternalMessageHandler.Instance.Config.WebSocketPort + 1}/apps/_UI/Default/SettingsKO.html";
        globalSettings.OverlayWebView._webView.WebView.LoadUrl(loadString);
        HasSettingsBeenOpenedOnce = true;
    }

    public static void RequestSettingsPatch(string sender, string data)
    {
        var pluginVersion = Plugin.PluginVersion;
#if DEBUG || DEV
        pluginVersion += " (Dev) ";
#endif
        if (SteamClient.IsValid && SteamApps.CurrentBetaName != null)
        {
            pluginVersion +=
                $" — you're on the <strong>{SteamApps.CurrentBetaName}</strong> branch of XSOverlay! Check the plugin repo releases tab for beta plugin updates/fixes";
        }
        else if (Tools.UpdateCheckResult.Key)
        {
            pluginVersion += $" — Update {Tools.UpdateCheckResult.Value} is available!";
        }

        // create new UiSettings instance
        var settings = new UiSettings
        {
            KBCheckForUpdates = PluginSettings.GetSetting<bool>("CheckForUpdates").Value,
            KBLiveSend = PluginSettings.GetSetting<bool>("LiveSend").Value,
            KBTypingIndicator = PluginSettings.GetSetting<bool>("TypingIndicator").Value,
            KBDisableMaxLength = PluginSettings.GetSetting<bool>("DisableMaxLength").Value,
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
                    $"http://localhost:{ExternalMessageHandler.Instance.Config.WebSocketPort + 1}/apps/_UI/Default/Settings.html";
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
            case "KBDisableMaxLength":
                PluginSettings.SetSetting<bool>("DisableMaxLength", value);
                break;
            case "KBOpenRepo":
                Application.OpenURL("https://github.com/nyakowint/xsoverlay-keyboard-osc");
                Tools.SendNotif("KeyboardOSC Github link opened in browser!");
                break;
            case "KBVersionCheck":
                Task.Run(Tools.CheckVersion);
                break;
        }

        return true;
    }

    public static bool BlockInputPatch(VirtualKeyCode keyCode)
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
            VirtualKeyCode.LEFT, VirtualKeyCode.RIGHT,
            
            // ime related keys
            VirtualKeyCode.KANA, VirtualKeyCode.KANJI, VirtualKeyCode.HANGUL,
            VirtualKeyCode.HANGEUL, VirtualKeyCode.HANJA, VirtualKeyCode.FINAL,
            VirtualKeyCode.JUNJA, VirtualKeyCode.CONVERT, VirtualKeyCode.NONCONVERT, 
            VirtualKeyCode.MODECHANGE, VirtualKeyCode.ACCEPT,
            VirtualKeyCode.PROCESSKEY,
        };
        return passthroughKeys.Contains(keyCode);
    }
}