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
                        "https://raw.githubusercontent.com/nyakowint/xsoverlay-keyboard-osc/NO-MORE-NODE/twitchAuth.html");
                var authJs =
                    client.DownloadString(
                        "https://raw.githubusercontent.com/nyakowint/xsoverlay-keyboard-osc/NO-MORE-NODE/twitchAuth.js");

                var filesPath = $"{Application.streamingAssetsPath}/Plugins/Applications/KeyboardOSC";
                if (!Directory.Exists(filesPath)) Directory.CreateDirectory(filesPath);
                Logger.LogInfo($"Writing auth page to: {filesPath}/twitchAuth.html and twitchAuth.js");
                File.WriteAllText($"{filesPath}/twitchAuth.html", authHtml);
                File.WriteAllText($"{filesPath}/twitchAuth.js", authJs);
            }
            catch (Exception exception)
            {
                Plugin.PluginLogger.LogError($"Error downloading auth page: {exception}");
                return;
            }
        }

        var authUrl = $"http://localhost:{port}/apps/KeyboardOSC/twitchAuth.html";
        Application.OpenURL(authUrl);
        const string confirmMsg = "Twitch auth started; Check your browser!";
        Logger.LogInfo(confirmMsg);
        Tools.SendBread(confirmMsg);
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