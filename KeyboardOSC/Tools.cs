using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using BepInEx;
using Newtonsoft.Json.Linq;
using UnityEngine;
using WindowsInput.Native;
using XSOverlay;
using XSOverlay.Websockets.API;
using Object = UnityEngine.Object;

namespace KeyboardOSC;

#pragma warning disable Publicizer001
public static class Tools
{
    
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

            logger.LogInfo($"Remote version: {remoteVersion}, Local version: {Plugin.PluginVersion}");
            
            if (Version.Parse(remoteVersion) > Version.Parse(Plugin.PluginVersion))
            {
                UpdateCheckResult = new KeyValuePair<bool, string>(true, remoteVersion);
                logger.LogInfo($"New version available! {remoteVersion}");
                ThreadingHelper.Instance.StartSyncInvoke(() => SendNotif("KeyboardChatbox update available!",
                    $"A new version of KeyboardOSC [ {remoteVersion} ] is available. You are currently using version {Plugin.PluginVersion}. :D"));
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

    public static bool DownloadModifiedUi()
    {
        var logger = Plugin.PluginLogger;
        if (Plugin.IsDebugConfig) return true;
        using var client = new WebClient();
        try
        {
            logger.LogInfo("Downloading modified HTML...");
            var htmlContent =
                client.DownloadString(
                    "https://raw.githubusercontent.com/nyakowint/xsoverlay-keyboard-osc/main/SettingsKO.html");
            logger.LogInfo("Downloading modified JS...");
            var jsContent =
                client.DownloadString(
                    "https://raw.githubusercontent.com/nyakowint/xsoverlay-keyboard-osc/main/settingsKO.js");
            
            var htmlPath = $"{Application.streamingAssetsPath}/Plugins/Applications/_UI/Default/Settings/SettingsKO.html";
            var jsPath = $"{Application.streamingAssetsPath}/Plugins/Applications/_UI/Default/_Shared/js/settingsKO.js";

            logger.LogInfo($"Writing settings HTML to: {htmlPath}");
            logger.LogInfo($"Writing settings JS to: {jsPath}");
            File.WriteAllText(htmlPath, htmlContent);
            File.WriteAllText(jsPath, jsContent);
        }
        catch (Exception exception)
        {
            Plugin.PluginLogger.LogError($"Exception downloading modified ui: {exception}");
            return false;
        }
        
        return true;
    }

    // didnt think id be reusing this old code lol
    public static Sprite GetSprite(this string resName)
    {
        var texture = GetTexture(resName);

        var rect = new Rect(0.0f, 0.0f, texture.width, texture.height);
        var pivot = new Vector2(0.5f, 0.5f);
        var border = Vector4.zero;
        var sprite = Sprite.CreateSprite_Injected(texture, ref rect, ref pivot, 100.0f, 0, SpriteMeshType.Tight,
            ref border, false);
        sprite.hideFlags |= HideFlags.DontUnloadUnusedAsset;
        return sprite;
    }

    public static Texture2D GetTexture(string resName)
    {
        var resourcePath = $"KeyboardOSC.{resName}.png";
        var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourcePath);
        if (stream == null)
        {
            Plugin.PluginLogger.LogError($"Resource \"{resourcePath}\" doesn't exist!");
            return null;
        }

        using var ms = new MemoryStream();
        stream.CopyTo(ms);

        var texture = new Texture2D(1, 1);
        texture.LoadImage(ms.ToArray());
        texture.hideFlags |= HideFlags.DontUnloadUnusedAsset;
        texture.wrapMode = TextureWrapMode.Clamp;

        return texture;
    }

    #region Region: System/Keys Methods

    public static string ConvertVirtualKeyToUnicode(VirtualKeyCode keyCode, uint scanCode, bool shift, bool altGr)
    {
        return GetCharsFromKeys(keyCode, scanCode, shift, altGr);
    }

    [DllImport("user32.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
    private static extern int ToUnicodeEx(uint wVirtKey, uint wScanCode, byte[] lpKeyState, StringBuilder pwszBuff,
        int cchBuff, uint wFlags, IntPtr dwhkl);


    // REEEEEEE
    private static string GetCharsFromKeys(VirtualKeyCode key, uint scanCode, bool shift, bool altGr)
    {
        StringBuilder stringBuilder = new StringBuilder(256);
        byte[] array = new byte[256];
        if (shift)
        {
            array[16] = byte.MaxValue;
        }

        if (altGr)
        {
            array[17] = byte.MaxValue;
            array[18] = byte.MaxValue;
        }

        int num = ToUnicodeEx((uint) key, scanCode, array, stringBuilder, stringBuilder.Capacity, 0u, _currentHkl);
        if (num == k_SUCCESS)
        {
            return stringBuilder.ToString();
        }

        if (num == k_NOTRANSLATION)
        {
            return "";
        }

        _ = k_DEADCHAR;
        return stringBuilder[stringBuilder.Length - 1].ToString();
    }

    public static int k_DEADCHAR;
    public static int k_NOTRANSLATION;
    public static int k_SUCCESS;
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value
    private static IntPtr _currentHkl;

    #endregion
}