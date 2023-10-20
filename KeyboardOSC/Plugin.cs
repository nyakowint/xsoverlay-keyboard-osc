using System;
using System.IO;
using System.Reflection;
using System.Threading;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using KeyboardOSC.XScripts;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using XSOverlay;

namespace KeyboardOSC
{
    [BepInPlugin("nwnt.keyboardosc", "KeyboardOSC", "1.0.1.1")]
    public class Plugin : BaseUnityPlugin
    {
        public static Plugin Instance;
        public static ManualLogSource PluginLogger;
        private static bool _isDebugConfig;
        public static bool IsChatModeActive;
        public Overlay_Manager overlayManager;
        public KeyboardInputHandler inputHandler;
        public GameObject chatButtonObj;
        public GameObject oscBarWindowObj;
        public GameObject oscBarCanvas;

        public static MethodInfo ReleaseStickyKeys;

        private void Awake()
        {
#if DEBUG
            _isDebugConfig = true;
#endif
            PluginLogger = Logger;
            if (Instance != null) Destroy(this);
            PluginSettings.ConfigFile = Config;
            PluginSettings.Init();
            if (!Environment.CommandLine.Contains("-batchmode") || _isDebugConfig) return;
            Logger.LogWarning("XSOverlay runs in batchmode normally (headless, without a window).");
            Logger.LogWarning("To see extended logs launch XSOverlay directly.");
        }

        private void Start()
        {
            Logger.LogWarning("It works! Starting Pre-setup");
            Instance = this;
            Console.Title = "XSO BepInEx Console";

            ReleaseStickyKeys = AccessTools.Method(typeof(KeyboardInputHandler), "ReleaseStickyKeys");
            Patches.PatchAll();

            ServerBridge.Instance.CommandMap["Keyboard"] = delegate
            {
                InitializeKeyboard();
                Overlay_Manager.Instance.EnableKeyboard();
            };
        }

        public void InitializeKeyboard()
        {
            // Plugin startup logic
            Logger.LogInfo("Initializing Keyboard OSC Plugin.");
            overlayManager = Overlay_Manager.Instance;

            inputHandler = overlayManager.Keyboard.GetComponent<KeyboardInputHandler>();
            SetupAdditionalGameObjects();

            ServerBridge.Instance.CommandMap["Keyboard"] = delegate { Overlay_Manager.Instance.EnableKeyboard(); };
        }

        private void SetupAdditionalGameObjects()
        {
            // Copy existing lock keyboard to create new button
            var keyboardOverlay = overlayManager.Keyboard;
            var keyboardWindow = overlayManager.Keyboard_Overlay;
            var keyboardWindowObj = overlayManager.Keyboard_Overlay.gameObject;

            var lockKeyboard = keyboardOverlay.GetComponentInChildren<LockKeyboardButton>(true).gameObject;
            chatButtonObj = Instantiate(lockKeyboard, lockKeyboard.transform.parent);
            chatButtonObj.DestroyComponent<LockKeyboardButton>();
            chatButtonObj.AddComponent<ToggleChatButton>();
            chatButtonObj.transform.Find("Image").GetComponent<Image>().sprite = "chat".GetSprite();
            chatButtonObj.Rename("OSC Keyboard Mode");


            // Create typing bar
            var oscBarRoot = new GameObject("TypingBarOverlay");
            oscBarRoot.SetActive(false);
            keyboardWindowObj.SetActive(false);
            oscBarRoot.AddComponent<OverlayTopLevelObject>();

            oscBarWindowObj = Instantiate(keyboardWindow.gameObject, oscBarRoot.transform);
            oscBarWindowObj.Rename("TypingBar Overlay");
            oscBarWindowObj.DestroyComponent<KeyboardGlobalManager>();
            oscBarWindowObj.DestroyComponent<ReparentToTarget>();
            oscBarWindowObj.AddComponent<ReparentBar>();

            var oscBarWindow = oscBarWindowObj.GetComponent<Unity_Overlay>();
            oscBarWindowObj.GetComponent<OverlayIdentifier>().OverlayTopLevelObject = oscBarRoot;
            oscBarWindow.overlayName = "chatbar";
            oscBarWindow.overlayKey = "chatbar";

            oscBarWindow.isMoveable = false;


            oscBarCanvas = Instantiate(keyboardOverlay.transform.Find("Keyboard Canvas | Manager"),
                oscBarRoot.transform, true).gameObject;
            var kbBackground = oscBarCanvas.transform.Find("Keyboard Background").gameObject;
            oscBarCanvas.Rename("OSCBar Canvas");
            oscBarCanvas.GetComponent<Canvas>().referencePixelsPerUnit = 420;
            Destroy(oscBarCanvas.transform.Find("KeyboardToolbar").gameObject);
            Destroy(oscBarCanvas.transform.Find("Keyboard Background/KeyboardLayout").gameObject);

            foreach (Transform child in oscBarCanvas.transform.Find("Keyboard Background/KeyboardSettings/Options"))
            {
                if (child.transform.GetSiblingIndex() > 0)
                {
                    Destroy(child.gameObject);
                }
            }

            var oscBarCameraObj = Instantiate(keyboardOverlay.GetComponentInChildren<Camera>().gameObject,
                oscBarRoot.transform);
            var oscBarCamera = oscBarCameraObj.GetComponent<Camera>();
            var camTrans = oscBarCamera.transform;
            var canvasTrans = oscBarCanvas.transform;
            oscBarCameraObj.Rename("Camera_OSCBar");

            oscBarCameraObj.DestroyComponent<UI_RescaleCameraToCanvas>();
            var rescaleToCanvas = oscBarCameraObj.AddComponent<RescaleCamToCanvas>();
            rescaleToCanvas.canvas = oscBarCanvas.GetComponent<RectTransform>();
            rescaleToCanvas.camera = oscBarCamera;

            // 100 1000
            oscBarWindow.renderTexHeightOverride = 60;
            oscBarWindow.renderTexWidthOverride = 730;
            oscBarWindow.widthInMeters = 0.55f;
            oscBarWindow.cameraForTexture = oscBarCamera;
            oscBarWindow.OverlayCanvas = oscBarCanvas.GetComponent<RectTransform>();

            oscBarWindow.canvasGraphicsCaster = oscBarCanvas.GetComponent<GraphicRaycaster>();

            oscBarRoot.transform.position = new Vector3(0.3f, 1, 0);

            var keyboardPos = Overlay_Manager.Instance.Keyboard_Overlay.transform.position;
            var obwTransform = oscBarWindow.transform;
            obwTransform.position = new Vector3(keyboardPos.x, keyboardPos.y + 0.01f, keyboardPos.z);
            obwTransform.rotation = new Quaternion(0, 0, 0, 0);

            XSOEventSystem.OnGrabbedOrDroppedOverlay += (overlay, _, grabbed) =>
            {
                if (overlay.overlayKey != "xso.overlay.keyboard" || grabbed) return;
                var newTrans = overlay.transform;
                var newPos = newTrans.position;
                const float offset = 0.155f;
                obwTransform.position = newPos + newTrans.up * offset;
                obwTransform.rotation = newTrans.rotation;
            };

            kbBackground.transform.localPosition = Vector3.zero;

            kbBackground.GetComponent<RectTransform>().sizeDelta = new Vector2(4130, 475);

            camTrans.position = canvasTrans.position;
            camTrans.rotation = canvasTrans.rotation;

            var oscBarTextObj = Instantiate(keyboardOverlay.transform
                .Find("Keyboard Canvas | Manager/Keyboard Background/KeyboardSettings/Options/Audio Toggle/Text (TMP)")
                .gameObject, kbBackground.transform);
            Destroy(oscBarCanvas.transform.Find("Keyboard Background/KeyboardSettings").gameObject);
            oscBarTextObj.Rename("TypingBar Text");
            var oscbarText = oscBarTextObj.GetComponent<TextMeshProUGUI>();
            XSTools.SetTMPUIText(oscbarText, "Type something silly! \"I bet you can't even read.\"");
            oscbarText.fontSize = 250f;
            oscbarText.fontSizeMax = 250f;
            oscbarText.horizontalAlignment = HorizontalAlignmentOptions.Center;
            oscbarText.verticalAlignment = VerticalAlignmentOptions.Middle;

            ChatModeManager.Setup(oscbarText);


            oscBarRoot.SetActive(true);
            keyboardWindowObj.SetActive(true);
        }

        public void ToggleChatMode()
        {
            var chatButton = chatButtonObj.GetComponent<Button>();
            var buttonColors = chatButton.colors;

            ReleaseStickyKeys.Invoke(inputHandler, null);

            IsChatModeActive = !IsChatModeActive;
            oscBarWindowObj.SetActive(IsChatModeActive);

            buttonColors.normalColor = (IsChatModeActive
                ? UIThemeHandler.Instance.T_HiTone
                : UIThemeHandler.Instance.T_DarkTone);
            chatButton.colors = buttonColors;
        }
    }
}