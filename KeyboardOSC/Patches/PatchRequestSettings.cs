using HarmonyLib;
using Steamworks;
using UnityEngine;
using XSOverlay.WebApp;
using XSOverlay.Websockets.API;

namespace KeyboardOSC.Patches;

[HarmonyPatch(typeof(ApiHandler), "OnRequestCurrentSettings")]
internal static class PatchRequestSettings
{
    [HarmonyPostfix]
    public static void Postfix(string sender)
    {
        if (!sender.Equals("systemui_settings")) return;
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

        var settings = new UiSettings
        {
            KBCheckForUpdates = PluginSettings.GetSetting<bool>("CheckForUpdates").Value,
            KBLiveSend = PluginSettings.GetSetting<bool>("LiveSend").Value,
            KBTypingIndicator = PluginSettings.GetSetting<bool>("TypingIndicator").Value,
            KBDisableMaxLength = PluginSettings.GetSetting<bool>("DisableMaxLength").Value,
            KBVersion = pluginVersion,
        };
        var data = JsonUtility.ToJson(settings, false);
        ServerClientBridge.Instance.Api.SendMessage("UpdateSettings", data, null, sender);
    }
}
