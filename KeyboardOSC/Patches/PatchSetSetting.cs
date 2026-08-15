using System.Threading.Tasks;
using HarmonyLib;
using XSOverlay;

namespace KeyboardOSC.Patches;

// Our settings ride along with XSOverlay's settings page, so they come through here.
[HarmonyPatch(typeof(XSettingsManager), nameof(XSettingsManager.SetSetting))]
internal static class PatchSetSetting
{
    [HarmonyPrefix]
    public static bool Prefix(string name, string value)
    {
        switch (name)
        {
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
                Tools.OpenRepo();
                break;
            case "KBVersionCheck":
                Task.Run(Tools.CheckVersion);
                break;
            default:
                return true;
        }

        // Keep the keyboard bar and settings page in sync, then skip XSOverlay's own handling
        ChatMode.PushConfig();
        return false;
    }
}
