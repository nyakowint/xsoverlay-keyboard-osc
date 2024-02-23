using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using PluginAppTest;
using XSOverlay;

[assembly: AssemblyVersion(Plugin.AssemblyVersion)]

namespace PluginAppTest
{
    [BepInPlugin("nwnt.pluginapptest", "PluginAppTest", AssemblyVersion)]
    public class Plugin : BaseUnityPlugin
    {
        public const string AssemblyVersion = "1.0.0";
        public static ManualLogSource PluginLogger;
        public Overlay_Manager overlayManager;

        private void Awake()
        {
            PluginLogger = Logger;
        }

        private void Start()
        {
            Logger.LogInfo($"PluginApp Test v{AssemblyVersion} - not real implementation!");
            Patches.PatchAll();
        }
    }
}