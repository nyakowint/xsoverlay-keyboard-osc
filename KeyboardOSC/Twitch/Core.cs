using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using BepInEx.Logging;
using Newtonsoft.Json;
using UnityEngine;
using XSOverlay;

namespace KeyboardOSC.Twitch;

public abstract class Core
{
    private static ManualLogSource Logger => Plugin.PluginLogger;

    public static bool IsTwitchSendingEnabled =>
        PluginSettings.GetSetting<bool>("TwitchSending").Value && Helix.CheckAccessToken();

    public static Tuple<string, string> GetAffixes()
    {
        if (PluginSettings.GetSetting<bool>("DisableAffixes").Value) return Tuple.Create("", "");
        return Tuple.Create(PluginSettings.GetSetting<string>("MsgPrefix").Value,
            PluginSettings.GetSetting<string>("MsgSuffix").Value);
    }
    private static void OpenAuthPage(int port = 42071)
    {
        if (!Plugin.IsDebugConfig)
        {
            using var client = new WebClient();
            try
            {
                Logger.LogInfo("Getting twitch auth page...");
                var authHtml =
                    client.DownloadString(
                        "https://raw.githubusercontent.com/nyakowint/xsoverlay-keyboard-osc/NO-MORE-NODE/TwitchAuth.html");

                var filesPath = $"{Application.streamingAssetsPath}/Plugins/Applications/KeyboardOSC";
                if (!Directory.Exists(filesPath)) Directory.CreateDirectory(filesPath);
                Logger.LogInfo($"Writing auth page to: {filesPath}/TwitchAuth.html");
                File.WriteAllText($"{filesPath}/TwitchAuth.html", authHtml);
            }
            catch (Exception exception)
            {
                Plugin.PluginLogger.LogError($"Error downloading auth page: {exception}");
                return;
            }
        }

        var authUrl = $"http://localhost:{port}/apps/KeyboardOSC/TwitchAuth.html";
        Application.OpenURL(authUrl);
        Logger.LogInfo("Twitch auth started; Check your browser!");
        Tools.SendBread("Twitch auth started; Check your browser!");
    }

    public static void SettingsCallback(string settingName, string value)
    {
        switch (settingName)
        {
            case "TwitchSetup":
                OpenAuthPage(ExternalMessageHandler.Instance.Config.WebSocketPort + 1);
                break;
            case "TwitchAuthCode":
                Task.Run(() => Helix.AuthenticateTwitch(JsonConvert.DeserializeObject<TwitchClientInfo>(value)));
                break;
        }
    }
}