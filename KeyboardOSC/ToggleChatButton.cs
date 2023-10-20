using BepInEx.Configuration;
using UnityEngine;
using UnityEngine.UI;

namespace KeyboardOSC;

public class ToggleChatButton : MonoBehaviour
{
    private void OnEnable()
    {
        GetComponent<Button>().onClick.AddListener(ToggleChatMode);
    }
    

    private void OnDisable()
    {
        GetComponent<Button>().onClick.RemoveAllListeners();
    }

    private static void ToggleChatMode()
    {
        PluginSettings.ConfigFile.TryGetEntry(PluginSettings.sectionId, "HasSeenHint", out ConfigEntry<bool> hasSeenHint);
        if (!hasSeenHint.Value)
        {
            Tools.SendBread("HOLD UP!", "Make sure OSC is enabled in VRChat or this will do nothing! lol");
            hasSeenHint.SetSerializedValue("true");
        }
        Plugin.Instance.ToggleChatMode();
    }
}