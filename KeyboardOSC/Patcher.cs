using System;
using HarmonyLib;
using KeyboardOSC.Patches;

namespace KeyboardOSC;

public static class Patcher
{
    private static Harmony _harmony;

    public static void PatchAll()
    {
        _harmony = new Harmony("nwnt.keyboardosc");
        
        var patchClasses = Plugin.IsVersionNewFangled
            ? new[]
            {
                typeof(PatchRequestSettings),
            }
            : new[]
            {
                typeof(PatchSendKey),
                typeof(PatchBlockInput),
                typeof(PatchScaleWindow),
                typeof(PatchAttachedMove),
                typeof(PatchAnalytics),
                typeof(PatchWebviewRegistration),
                typeof(PatchSetSetting),
                typeof(PatchRequestSettings),
            };

        foreach (var patch in patchClasses)
        {
            try
            {
                Plugin.PluginLogger.LogInfo($"[KBOSC:Patches] Applying {patch.Name}");
                _harmony.PatchAll(patch);
                Plugin.PluginLogger.LogInfo($"[KBOSC:Patches] {patch.Name} applied");
            }
            catch (Exception e)
            {
                Plugin.PluginLogger.LogWarning($"[KBOSC:Patches] {patch.Name} failed, skipping: {e.Message}");
            }
        }
    }
}