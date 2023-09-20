using UnityEngine;
using XSOverlay;

namespace KeyboardOSC.XScripts;

public class ReparentBar : MonoBehaviour
{
    private GameObject _target;

    private void Start()
    {
        _target = Overlay_Manager.Instance.Keyboard_Overlay.gameObject;
        transform.SetParent(_target.transform);
    }
}