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
using HarmonyLib; 
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

    static Tools()
    {
        // Initialize keyboard layout once on load
        RefreshKeyboardLayout();
    }

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
                ThreadingHelper.Instance.StartSyncInvoke(() => SendNotif("KeyboardChatbox update available!",
                    $"A new version of KeyboardOSC [ {remoteVerObj} ] is available. You are currently using version {Plugin.PluginVersion}. :D"));
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
    
    public static Sprite GetSprite(this string resName)
    {
        var texture = GetTexture(resName);
        if (!texture)
        {
            // Fallback: create a 1x1 transparent texture so callers don't NRE
            texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, Color.clear);
            texture.Apply();
        }

        var rect = new Rect(0.0f, 0.0f, texture.width, texture.height);
        var pivot = new Vector2(0.5f, 0.5f);
        var border = Vector4.zero;
        var sprite = Sprite.Create(texture, rect, pivot, 100.0f, 0, SpriteMeshType.Tight, border, false);
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

    // Reflection safety helpers
    public static MethodInfo SafeMethod(Type type, string name, Type[] args = null, bool required = false)
    {
        try
        {
            var mi = AccessTools.Method(type, name, args);
            if (mi == null)
            {
                Plugin.PluginLogger.LogWarning($"[Reflection] Method not found: {type.FullName}.{name}");
                if (required)
                    Plugin.PluginLogger.LogError($"Required method missing; related feature will be disabled.");
            }
            return mi;
        }
        catch (Exception ex)
        {
            Plugin.PluginLogger.LogError($"[Reflection] Error retrieving method {type.FullName}.{name}: {ex.Message}");
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
                Plugin.PluginLogger.LogWarning($"[Reflection] Field not found: {type.FullName}.{name}");
                if (required)
                    Plugin.PluginLogger.LogError($"Required field missing; related feature will be disabled.");
            }
            return fi;
        }
        catch (Exception ex)
        {
            Plugin.PluginLogger.LogError($"[Reflection] Error retrieving field {type.FullName}.{name}: {ex.Message}");
            return null;
        }
    }

    [DllImport("user32.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
    private static extern int ToUnicodeEx(uint wVirtKey, uint wScanCode, byte[] lpKeyState, StringBuilder pwszBuff,
        int cchBuff, uint wFlags, IntPtr dwhkl);

    [DllImport("user32.dll")]
    private static extern IntPtr GetKeyboardLayout(uint idThread);

    public static void RefreshKeyboardLayout()
    {
        try
        {
            _currentHkl = GetKeyboardLayout(0);
            Plugin.PluginLogger?.LogInfo($"[Keyboard] Active layout HKL: 0x{_currentHkl.ToInt64():X16}");
        }
        catch (Exception ex)
        {
            Plugin.PluginLogger?.LogWarning($"Failed to refresh keyboard layout: {ex.Message}");
        }
    }

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