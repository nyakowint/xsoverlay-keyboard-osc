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
        if (Plugin.IsFirstOpen)
        {
            Tools.SendBread("HOLD UP!", "Make sure OSC is enabled in VRChat or this will do nothing! lol");
            Plugin.IsFirstOpen = false;
        }
        Plugin.Instance.ToggleChatMode();
    }
}