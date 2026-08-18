using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using XSOverlay;
using XSOverlay.Websockets.API;
using Object = UnityEngine.Object;

namespace KeyboardOSC;

#pragma warning disable Publicizer001
public static class Tools
{
    public const string RepoUrl = "https://github.com/nyakowint/xsoverlay-keyboard-osc";

    public static KeyValuePair<bool, string> UpdateCheckResult = new(false, "");

    public static void SendOsc(string address, params object[] msg)
    {
        var oscClient = ExternalMessageHandler.Instance.OscClient;
        if (oscClient.isRunning)
        {
            oscClient.Send(address, msg);
        }
        else
        {
            SendNotif("Oops!", $"Failed to send OSC to {address}, client not running?");
            Plugin.PluginLogger.LogWarning("Failed to send OSC message, client is not running!");
        }
    }

    public static void SendNotif(string title, string content = "")
    {
        var notif = new Objects.NotificationObject
        {
            title = title,
            content = content,
            messageType = 1,
            timeout = 5f,
            height = CalculateHeight(content),
            sourceApp = "KeyboardOSC Plugin",
            volume = 0.5f
        };
        XSOEventSystem.Current.EventQueueNotification(notif);
    }

    public static void OpenRepo()
    {
        Application.OpenURL(RepoUrl);
        SendNotif("KeyboardOSC Github link opened in browser!");
    }

    private static int CalculateHeight(string content)
    {
        return content.Length switch
        {
            <= 100 => 100,
            <= 200 => 150,
            <= 300 => 200,
            _ => 250
        };
    }

    #region Region: Unity Extensions

    public static void DestroyComponent<T>(this GameObject go) where T : Component
    {
        if (go != null)
        {
            Object.Destroy(go.GetComponent<T>());
        }
        else
        {
            Plugin.PluginLogger.LogError(
                $"Could not DestroyComponent of {typeof(T).Name} as target GameObject was null");
        }
    }

    public static void Rename(this Object go, string newName)
    {
        if (go != null)
        {
            go.name = newName;
        }
        else
        {
            Plugin.PluginLogger.LogError($"Could not Rename {nameof(go)} as target Object was null");
        }
    }

    #endregion

    private const string VersionUrl = "https://raw.githubusercontent.com/nyakowint/xsoverlay-keyboard-osc/main/VERSION";

    public static async Task CheckVersion()
    {
        var logger = Plugin.PluginLogger;
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Add("User-Agent", "xso-kbosc");

        logger.LogInfo("Checking for plugin updates...");
        try
        {
            var response = await client.GetStringAsync(VersionUrl);
            var remoteVersion = response.Trim();
            if (string.IsNullOrEmpty(remoteVersion))
            {
                logger.LogError("VERSION file is empty or invalid.");
                return;
            }

            if (remoteVersion.StartsWith("v", StringComparison.OrdinalIgnoreCase))
                remoteVersion = remoteVersion.Substring(1);

            if (!Version.TryParse(remoteVersion, out var remoteVerObj))
            {
                logger.LogError($"Failed to parse remote version string: '{remoteVersion}'");
                return;
            }

            if (!Version.TryParse(Plugin.PluginVersion, out var localVerObj))
            {
                logger.LogWarning($"Local plugin version '{Plugin.PluginVersion}' could not be parsed, skipping comparison.");
                return;
            }

            logger.LogInfo($"Remote version: {remoteVerObj}, Local version: {localVerObj}");

            if (remoteVerObj > localVerObj)
            {
                UpdateCheckResult = new KeyValuePair<bool, string>(true, remoteVerObj.ToString());
                logger.LogInfo($"New version available! {remoteVerObj}");
                ThreadingHelper.Instance.StartSyncInvoke(() =>
                {
                    SendNotif("KeyboardChatbox Update available!",
                        $"A new version of Keyboard Chatbox [ {remoteVerObj} ] is available. You are currently using version {Plugin.PluginVersion}. :D");
                    ChatMode.PushConfig();
                });
            }
            else
            {
                logger.LogInfo("No updates available.");
            }
        }
        catch (Exception ex)
        {
            logger.LogError($"Failed to check for updates: {ex.Message}");
        }
    }

    private static readonly Dictionary<string, string> InjectedScripts = new()
    {
        { "KeyboardOSC.chatbox-settings.js", "chatbox-settings.js" }
    };

    public static string ScriptPath(string fileName)
    {
        return $"{Application.streamingAssetsPath}/Plugins/Applications/_UI/Default/_Shared/js/{fileName}";
    }

    public static bool WriteInjectedUi()
    {
        var logger = Plugin.PluginLogger;
        var success = true;

        foreach (var script in InjectedScripts)
        {
            try
            {
                var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(script.Key);
                if (stream == null)
                {
                    logger.LogError($"Embedded resource \"{script.Key}\" not found!");
                    success = false;
                    continue;
                }

                using var reader = new StreamReader(stream);
                var jsContent = reader.ReadToEnd();

                var jsPath = ScriptPath(script.Value);
                logger.LogInfo($"Writing embedded UI script to: {jsPath}");
                File.WriteAllText(jsPath, jsContent);
            }
            catch (Exception exception)
            {
                logger.LogError($"Exception writing embedded UI ({script.Value}): {exception}");
                success = false;
            }
        }

        try
        {
            var legacyScript = ScriptPath("settings-chatbox.js");
            if (File.Exists(legacyScript)) File.Delete(legacyScript);
        }
        catch (Exception exception)
        {
            logger.LogWarning($"Couldn't clean up the old settings script: {exception.Message}");
        }

        return success;
    }

    #region Region: Reflection safety helpers

    public static MethodInfo SafeMethod(Type type, string name, Type[] args = null, bool required = false)
    {
        try
        {
            var mi = AccessTools.Method(type, name, args);
            if (mi == null)
            {
                Plugin.PluginLogger.LogWarning($"[KBOSC:Reflection] Method not found: {type.FullName}.{name}");
                if (required)
                    Plugin.PluginLogger.LogError("Required method missing; related feature will be disabled.");
            }
            return mi;
        }
        catch (Exception ex)
        {
            Plugin.PluginLogger.LogError($"[KBOSC:Reflection] Error retrieving method {type.FullName}.{name}: {ex.Message}");
            return null;
        }
    }

    public static FieldInfo SafeField(Type type, string name, bool required = false)
    {
        try
        {
            var fi = AccessTools.Field(type, name);
            if (fi == null)
            {
                Plugin.PluginLogger.LogWarning($"[KBOSC:Reflection] Field not found: {type.FullName}.{name}");
                if (required)
                    Plugin.PluginLogger.LogError("Required field missing; related feature will be disabled.");
            }
            return fi;
        }
        catch (Exception ex)
        {
            Plugin.PluginLogger.LogError($"[KBOSC:Reflection] Error retrieving field {type.FullName}.{name}: {ex.Message}");
            return null;
        }
    }

    #endregion
}
