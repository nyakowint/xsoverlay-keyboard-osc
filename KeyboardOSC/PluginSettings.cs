using System.Threading.Tasks;
using BepInEx.Configuration;
using KeyboardOSC.Twitch;

// ReSharper disable InconsistentNaming

namespace KeyboardOSC;

public abstract class PluginSettings
{
    public static ConfigFile ConfigFile;
    public static string sectionId = "KBOSC";

    internal static void Init()
    {
        ConfigFile.Bind(sectionId, "HasSeenHint", false, "Has user seen osc hint");
        ConfigFile.Bind(sectionId, "CheckForUpdates", false, "Notify user of updates on init?");
        ConfigFile.Bind(sectionId, "LiveSend", false, "Send messages as you're typing them");
        ConfigFile.Bind(sectionId, "TypingIndicator", true, "Whether to send typing indicator");
        
        
        ConfigFile.Bind(sectionId, "TwitchSending", false, "Send messages to your twitch chat? (requires setup)");
        ConfigFile.Bind(sectionId, "MsgPrefix", "", "Message prefix sent with each message");
        ConfigFile.Bind(sectionId, "MsgSuffix", "", "Message suffix sent with each message");
        ConfigFile.Bind(sectionId, "DisableAffixes", false, "Disable both prefix and suffix (e.g. playing a vr game other than vrchat)");
        var tClient = ConfigFile.Bind(sectionId, "TwitchClientId", "", "Your twitch app client ID used for chat sending. Don't set these manually");
        var tSecret = ConfigFile.Bind(sectionId, "TwitchClientSecret", "", "Twitch client secret");
        var tRefresh = ConfigFile.Bind(sectionId, "TwitchRefreshToken", "", "Twitch refresh token");
        
        if (!string.IsNullOrEmpty(tClient.Value) && !string.IsNullOrEmpty(tSecret.Value) && !string.IsNullOrEmpty(tRefresh.Value))
        {
            Task.Run(Helix.RefreshTokens);
        }
    }

    public static ConfigEntry<T> GetSetting<T>(string settingName)
    {
        ConfigFile.TryGetEntry<T>(sectionId, settingName, out var configEntry);
        return configEntry;
    }

    public static void SetSetting<T>(string settingName, string value)
    {
        ConfigFile.TryGetEntry<T>(sectionId, settingName, out var configEntry);
        configEntry.SetSerializedValue(value);
    }
}

public class UiSettings
{
    public string KBVersion = Plugin.AssemblyVersion;
    public bool KBCheckForUpdates = false;
    public bool KBLiveSend = false;
    public bool KBTypingIndicator = true;
    public bool KBTwitchSending = false;
}