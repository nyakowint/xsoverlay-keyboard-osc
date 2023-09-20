using UnityEngine;

namespace KeyboardOSC.XScripts;

// Sourced from decompiled XSOverlay Beta 616, editor scripts moment
public class RescaleCamToCanvas : MonoBehaviour
{
    public RectTransform canvas;
    public Camera camera;

    private void OnEnable()
    {
        if (canvas == null) canvas = transform.parent.GetComponentInChildren<RectTransform>();
        if (camera == null) camera = GetComponent<Camera>();
        RescaleCamera();
    }

    private void RescaleCamera()
    {
        var array = new Vector3[4];
        if (canvas == null) 
        {
            Plugin.PluginLogger.LogWarning("uihhhhhhhhhmmmmm yikes");
            return;
        }
        canvas.GetWorldCorners(array);
        var num = Vector3.Distance(array[0], array[1]);
        camera.orthographicSize = num / 2f;
    }
}