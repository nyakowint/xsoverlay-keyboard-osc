using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;
using XSOverlay;

namespace KeyboardOSC.Patches;

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
                Application.OpenURL("https://github.com/nyakowint/xsoverlay-keyboard-osc");
                Tools.SendNotif("KeyboardOSC Github link opened in browser!");
                break;
            case "KBVersionCheck":
                Task.Run(Tools.CheckVersion);
                break;
        }

        return true;
    }
}
