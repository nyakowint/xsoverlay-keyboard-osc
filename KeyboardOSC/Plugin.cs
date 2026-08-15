using System;
using System.Collections;
using System.Reflection;
using System.Threading.Tasks;
using BepInEx;
using BepInEx.Logging;
using UnityEngine;
using XSOverlay.WebApp;

[assembly: AssemblyVersion("2.0.0")]

namespace KeyboardOSC
{
    [BepInPlugin("nwnt.keyboardosc", "KeyboardOSC", PluginVersion)]
    public class Plugin : BaseUnityPlugin
    {
        public const string PluginVersion = "2.0.0";
        public static Plugin Instance;
        public static ManualLogSource PluginLogger;

        public static bool IsDebugConfig = false;
        public static bool IsDevBuild;

        // Set from the keyboard webview whenever the chat bar owns keyboard input focus.
        public static bool ChatModeActive;
        public static bool ModifiedUiSuccess;

        private void Awake()
        {
#if DEBUG
            IsDebugConfig = true;
#elif DEV
            Logger.LogWarning("!! DEVELOPMENT BUILD !! ");
            Logger.LogWarning("YOU ARE USING A DEVELOPMENT BUILD AND THINGS MAY NOT WORK RIGHT!!");
            Logger.LogWarning("!! DEVELOPMENT BUILD !! ");
                IsDebugConfig = true;
                IsDevBuild = true;
#endif
            Instance = this;
            PluginLogger = Logger;
            PluginSettings.ConfigFile = Config;
            PluginSettings.Init();

            ModifiedUiSuccess = Tools.WriteInjectedUi();
            // Subscribed before any webview finishes loading so the first load gets patched too
            UiInjector.Subscribe();

            if (!Environment.CommandLine.Contains("-batchmode") || IsDebugConfig) return;
            Logger.LogWarning("XSOverlay runs in batchmode normally (headless without a window).");
            Logger.LogWarning("To see extended logs launch XSOverlay directly.");
        }

        private void Start()
        {
            Logger.LogInfo($"Keyboard OSC v{PluginVersion} started!");
            Logger.LogWarning("Report plugin-specific issues to the GitHub repo.");
            Logger.LogWarning("!! / Please remove KeyboardChatbox before reporting bugs to XSOverlay developers! \\ !!");
            Console.Title = "KeyboardOSC - XSOverlay";

            Patcher.PatchAll();
            StartCoroutine(SetupBridge());
        }

        private void OnDestroy()
        {
            UiInjector.Unsubscribe();
        }

        // ServerClientBridge builds its api on Awake, but plugin load order isn't guaranteed
        private IEnumerator SetupBridge()
        {
            var waited = 0f;
            while (ServerClientBridge.Instance?.Api == null)
            {
                if (waited > 30f)
                {
                    Logger.LogError("[CRITICAL] XSOverlay's websocket api never appeared, chatbox will not work!");
                    yield break;
                }

                waited += Time.unscaledDeltaTime;
                yield return null;
            }

            ChatMode.RegisterCommands(ServerClientBridge.Instance.Api);
            Logger.LogInfo("Chatbox bridge registered!");

            // Covers pages that finished loading before we were around to hear about it
            UiInjector.InjectExisting();

            if (PluginSettings.GetSetting<bool>("CheckForUpdates").Value) Task.Run(Tools.CheckVersion);
        }
    }
}
