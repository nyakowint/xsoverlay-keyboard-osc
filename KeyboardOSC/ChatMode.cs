using System;
using System.Collections.Generic;
using BepInEx.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Steamworks;
using UnityEngine;
using XSOverlay.WebApp;
using XSOverlay.Websockets.API;

namespace KeyboardOSC;

public static class ChatMode
{
    public const int MaxLength = 144;
    
    public const string SettingsClient = "systemui_settings";

    private const string CmdReady = "KBOSCReady";
    private const string MsgConfig = "KBOSCConfig";

    private static ApiHandler _api;

    public static void RegisterCommands(ApiHandler api)
    {
        _api = api;
        api.Commands[CmdReady] = (sender, _, _) => SendConfig(sender);
    }

    #region Region: Outgoing messages

    public static void PushConfig()
    {
        SendConfig(SettingsClient);
    }

    private static void SendConfig(string client)
    {
        var config = BuildConfig();
        var json = JsonConvert.SerializeObject(config);
        Plugin.PluginLogger.LogInfo($"Sending config to {client}");
        SendMessage(MsgConfig, json, client);
    }

    private static ChatboxConfig BuildConfig()
    {
        return new ChatboxConfig
        {
            version = Plugin.PluginVersion,
            versionText = BuildVersionText(),
            liveSend = PluginSettings.GetSetting<bool>("LiveSend").Value,
            typingIndicator = PluginSettings.GetSetting<bool>("TypingIndicator").Value,
            disableMaxLength = PluginSettings.GetSetting<bool>("DisableMaxLength").Value,
            checkForUpdates = PluginSettings.GetSetting<bool>("CheckForUpdates").Value,
            maxLength = MaxLength,
            updateAvailable = Tools.UpdateCheckResult.Key
        };
    }

    private static string BuildVersionText()
    {
        var text = $"Version {Plugin.PluginVersion}";
#if DEBUG || DEV
        text += " Dev";
#endif
        try
        {
            if (SteamClient.IsValid && !string.IsNullOrEmpty(SteamApps.CurrentBetaName))
            {
                text +=
                    ". !! This version has official chatbox support! You can remove the plugin now !!";
                return text;
            }
        }
        catch (Exception ex)
        {
            Plugin.PluginLogger.LogWarning($"Couldn't read the current steam branch: {ex.Message}");
        }

        if (Tools.UpdateCheckResult.Key) text += $" — Update {Tools.UpdateCheckResult.Value} is available!";
        return text;
    }

    private static void SendMessage(string command, string json, string client)
    {
        if (_api == null) return;
        if (!_api.SystemClients.ContainsKey(client)) return;

        try
        {
            _api.SendMessage(command, json, null, client);
        }
        catch (Exception ex)
        {
            Plugin.PluginLogger.LogError($"Failed to send {command} to {client}: {ex.Message}");
        }
    }

    #endregion
}

[Serializable]
public class ChatboxConfig
{
    public string version;
    public string versionText;
    public bool liveSend;
    public bool typingIndicator;
    public bool disableMaxLength;
    public bool checkForUpdates;
    public bool updateAvailable;
    public int maxLength;
    public Shortcode[] macros;
}

[Serializable]
public class Shortcode
{
    public string code;
    public string glyph;
}

[Serializable]
public class ClipboardData
{
    public string text;
}
