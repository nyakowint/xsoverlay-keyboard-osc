using BepInEx.Configuration;

// ReSharper disable InconsistentNaming

namespace KeyboardOSC;

public class PluginSettings
{
    public static ConfigFile ConfigFile;
    public static string sectionId = "KBOSC";

    internal static void Init()
    {
        ConfigFile.Bind(sectionId, "HasSeenHint", false, "Has user seen osc hint");
        ConfigFile.Bind(sectionId, "CheckForUpdates", false, "Notify user of updates on init?");
        ConfigFile.Bind(sectionId, "LiveSend", false, "Notify user of updates on init?");
        ConfigFile.Bind(sectionId, "TypingIndicator", true, "Notify user of updates on init?");
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
    public bool KBCheckForUpdates = false;
    public bool KBLiveSend = false;
    public bool KBTypingIndicator = true;
}