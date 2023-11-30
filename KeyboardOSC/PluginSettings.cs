using BepInEx.Configuration;

namespace KeyboardOSC;

public class PluginSettings
{
    public static ConfigFile ConfigFile;
    public static string sectionId = "KBOSC";

    internal static void Init()
    {
        ConfigFile.Bind(sectionId, "HasSeenHint", false, "Has user seen osc hint");
        ConfigFile.Bind(sectionId, "CheckForUpdates", false, "Notify user of updates on init?");
    }
}