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

/// <summary>
/// Bridges the chat bar injected into XSOverlay's keyboard webview to VRChat's OSC chatbox.
/// The webview owns all of the text/UI state, this side only talks OSC and plugin settings.
/// </summary>
public static class ChatMode
{
    public const int MaxLength = 144;

    public const string KeyboardClient = "systemui_keyboard";
    public const string SettingsClient = "systemui_settings";

    // Commands coming in from the injected javascript
    private const string CmdReady = "KBOSCReady";
    private const string CmdState = "KBOSCState";
    private const string CmdSend = "KBOSCSend";
    private const string CmdTyping = "KBOSCTyping";
    private const string CmdClipboard = "KBOSCClipboard";

    // Messages going back out to it
    private const string MsgConfig = "KBOSCConfig";
    private const string MsgClipboard = "KBOSCClipboardData";

    private static ApiHandler _api;

    public static void RegisterCommands(ApiHandler api)
    {
        _api = api;
        api.Commands[CmdReady] = (sender, _, _) => SendConfig(sender);
        api.Commands[CmdState] = (_, json, _) => OnChatState(json);
        api.Commands[CmdSend] = (_, json, _) => OnSendChat(json);
        api.Commands[CmdTyping] = (_, json, _) => OnTyping(json);
        api.Commands[CmdClipboard] = (sender, json, _) => OnClipboard(sender, json);
    }

    #region Region: Incoming commands

    private static void OnChatState(string json)
    {
        var active = ParseJson(json)?["active"]?.Value<bool>() ?? false;
        if (active == Plugin.ChatModeActive) return;

        Plugin.ChatModeActive = active;
        Plugin.PluginLogger.LogInfo($"Chat mode {(active ? "enabled" : "disabled")}");

        if (active)
        {
            ShowFirstTimeHint();
            return;
        }

        SendTyping(false);
    }

    private static void OnSendChat(string json)
    {
        var data = ParseJson(json);
        if (data == null) return;

        var text = data["text"]?.Value<string>() ?? string.Empty;
        var sfx = data["sfx"]?.Value<bool>() ?? true;

        text = ReplaceShortcodes(text);
        if (!PluginSettings.GetSetting<bool>("DisableMaxLength").Value && text.Length > MaxLength)
        {
            text = text.Substring(0, MaxLength);
        }

#if DEBUG
        Plugin.PluginLogger.LogInfo($"Sending message (sfx: {sfx}): {text}");
#endif
        InputToChatbox(text, sfx);
    }

    private static void OnTyping(string json)
    {
        SendTyping(ParseJson(json)?["typing"]?.Value<bool>() ?? false);
    }

    // The webview has no clipboard access of its own, so it asks us to do it
    private static void OnClipboard(string sender, string json)
    {
        var data = ParseJson(json);
        var action = data?["action"]?.Value<string>();

        switch (action)
        {
            case "copy":
                GUIUtility.systemCopyBuffer = data["text"]?.Value<string>() ?? string.Empty;
                break;
            case "paste":
                SendMessage(MsgClipboard, JsonConvert.SerializeObject(new ClipboardData
                {
                    text = GUIUtility.systemCopyBuffer ?? string.Empty
                }), sender);
                break;
            default:
                Plugin.PluginLogger.LogWarning($"Unknown clipboard action requested: {action}");
                break;
        }
    }

    #endregion

    #region Region: OSC

    /// <summary>
    /// Since i keep forgetting:
    /// /chatbox/input s b n Input text into the chatbox.
    /// If B is True, send the text in S immediately, bypassing the keyboard. If b is False, open the keyboard and populate it with the provided text.
    /// N is an additional bool parameter that when set to False will not trigger the notification SFX (defaults to True if not specified).
    /// </summary>
    private static void InputToChatbox(string text, bool triggerSfx = true)
    {
        Tools.SendOsc("/chatbox/input", text, true, triggerSfx);
    }

    private static void SendTyping(bool typing)
    {
        Tools.SendOsc("/chatbox/typing", typing);
    }

    #endregion

    #region Region: Outgoing messages

    /// <summary>Pushes current plugin settings to the keyboard and settings pages.</summary>
    public static void PushConfig()
    {
        SendConfig(KeyboardClient);
        SendConfig(SettingsClient);
    }

    private static void SendConfig(string client)
    {
        var config = BuildConfig();
        // JsonUtility is fussy about nested arrays, and this is the serializer XSOverlay's own
        // api objects go through anyway
        var json = JsonConvert.SerializeObject(config);
        Plugin.PluginLogger.LogInfo($"Sending config to {client} ({config.macros?.Length ?? 0} macros)");
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
            updateAvailable = Tools.UpdateCheckResult.Key,
            macros = ShortcodeList()
        };
    }

    private static string BuildVersionText()
    {
        var text = $"Version {Plugin.PluginVersion}";
#if DEBUG || DEV
        text += " (Dev)";
#endif
        try
        {
            if (SteamClient.IsValid && !string.IsNullOrEmpty(SteamApps.CurrentBetaName))
            {
                text +=
                    $" — you're on the <strong>{SteamApps.CurrentBetaName}</strong> branch of XSOverlay! Check the plugin repo releases tab for beta plugin updates/fixes";
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
        // Sending to a page that isn't up yet only produces websocket noise
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

    private static void ShowFirstTimeHint()
    {
        PluginSettings.ConfigFile.TryGetEntry(PluginSettings.sectionId, "HasSeenHint",
            out ConfigEntry<bool> hasSeenHint);
        if (hasSeenHint == null || hasSeenHint.Value) return;

        Tools.SendNotif("HOLD UP!", "Make sure OSC is enabled in VRChat or this will do nothing! lol");
        hasSeenHint.Value = true;
    }

    private static JObject ParseJson(string json)
    {
        if (string.IsNullOrEmpty(json)) return null;
        try
        {
            return JObject.Parse(json);
        }
        catch (Exception ex)
        {
            Plugin.PluginLogger.LogError($"Malformed payload from the keyboard webview: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Text macros, typed as shortcodes or picked from the chat bar's macro menu. The keyboard
    /// page builds that menu from this list, so this stays the only place they're defined.
    /// </summary>
    private static readonly Dictionary<string, string> Shortcodes = new()
    {
        // //hrt2 and //skull2 have to be replaced before their shorter namesakes
        { "//shrug", "¯\\_(ツ)_/¯" },
        { "//happy", "(¬‿¬)" },
        { "//tflip", "┬─┬" },
        { "//music", "🎵" },
        { "//cookie", "🍪" },
        { "//star", "⭐" },
        { "//hrt2", "💕" },
        { "//hrt", "💗" },
        { "//skull2", "☠" },
        { "//skull", "💀" },
        { "//rx3", "rawr x3" }
    };

    private static Shortcode[] ShortcodeList()
    {
        var list = new List<Shortcode>();
        foreach (var shortcode in Shortcodes)
        {
            list.Add(new Shortcode { code = shortcode.Key, glyph = shortcode.Value });
        }

        return list.ToArray();
    }

    private static string ReplaceShortcodes(this string input)
    {
        foreach (var shortcode in Shortcodes)
        {
            input = input.Replace(shortcode.Key, shortcode.Value);
        }

        return input;
    }
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
