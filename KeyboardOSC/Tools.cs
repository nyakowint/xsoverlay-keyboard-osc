using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;
using WindowsInput.Native;
using XSOverlay;
using Object = UnityEngine.Object;

namespace KeyboardOSC;

#pragma warning disable Publicizer001
public static class Tools
{
    public static void SendOsc(string address, params object[] msg)
    {
        var oscClient = ExternalMessageHandler.Instance.OscClient;
        if (oscClient.isRunning)
        {
            oscClient.Send(address, msg);
        }
        else
        {
            SendBread("Oops!", $"Failed to send OSC to {address}, client not running?");
            Plugin.PluginLogger.LogWarning("Failed to send OSC message, client is not running!");
        }
    }

    public static void SendBread(string title, string content)
    {
        var notif = new XSOMessage()
        {
            title = title,
            content = content,
            messageType = 1,
            timeout = 5f,
            sourceApp = "Keyboard Plugin",
            volume = 0.5f
        };
        NotificationHandler.Instance.PrepareToast(notif);
    }
    

    public static string ConvertVirtualKeyToUnicode(VirtualKeyCode keyCode, uint scanCode, bool shift)
    {
        return GetCharsFromKeys(keyCode, scanCode, shift, false);
    }

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
        int num = ToUnicodeEx((uint)key, scanCode, array, stringBuilder, stringBuilder.Capacity, 0u, _currentHkl);
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
}