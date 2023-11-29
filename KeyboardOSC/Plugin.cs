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
using UnityEngine.Serialization;
using UnityEngine.UI;
using XSOverlay;

namespace KeyboardOSC
{
    [BepInPlugin("nwnt.keyboardosc", "KeyboardOSC", "1.1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        public static Plugin Instance;
        public static ManualLogSource PluginLogger;
        private static bool _isDebugConfig;
        public static bool IsChatModeActive;
        public Overlay_Manager overlayManager;
        public KeyboardInputHandler inputHandler;
        
        public GameObject toggleBarObj;

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
            Instance = this;
            Logger.LogInfo("hi!");
            Console.Title = "KeyboardOSC - XSOverlay";

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
            Logger.LogInfo("Keyboard opened for the first time, initializing KeyboardOSC Plugin.");
            overlayManager = Overlay_Manager.Instance;
            inputHandler = overlayManager.Keyboard.GetComponent<KeyboardInputHandler>();

            SetupToggleButton();
            SetupBar();

            ServerBridge.Instance.CommandMap["Keyboard"] = delegate { Overlay_Manager.Instance.EnableKeyboard(); };
        }

        private void SetupToggleButton()
        {
            var lockButtonObj = overlayManager.Keyboard.GetComponentInChildren<LockKeyboardButton>(true).gameObject;
            toggleBarObj = Instantiate(lockButtonObj, lockButtonObj.transform.parent);
            toggleBarObj.DestroyComponent<LockKeyboardButton>();
            toggleBarObj.AddComponent<ToggleChatButton>();
            toggleBarObj.transform.Find("Image").GetComponent<Image>().sprite = "chat".GetSprite();
            toggleBarObj.Rename("KeyboardOSC Toggle");
        }

        private void SetupBar()
        {
            var keyboard = overlayManager.Keyboard;
            var keyboardWindow = overlayManager.Keyboard_Overlay;
            var keyboardWindowObj = overlayManager.Keyboard_Overlay.gameObject;
            keyboardWindowObj.SetActive(false);

            // Create typing bar
            var oscBarRoot = new GameObject("KeyboardOSC Root");
            oscBarRoot.SetActive(false);
            oscBarRoot.AddComponent<OverlayTopLevelObject>();


            oscBarWindowObj = Instantiate(keyboardWindow.gameObject, oscBarRoot.transform);
            oscBarWindowObj.Rename("KeyboardOSC Window");
            oscBarWindowObj.DestroyComponent<KeyboardGlobalManager>();
            oscBarWindowObj.DestroyComponent<ReparentToTarget>();
            oscBarWindowObj.AddComponent<ReparentBar>();

            var oscBarWindow = oscBarWindowObj.GetComponent<Unity_Overlay>();
            oscBarWindowObj.GetComponent<OverlayIdentifier>().OverlayTopLevelObject = oscBarRoot;
            oscBarWindow.overlayName = "chatbar";
            oscBarWindow.overlayKey = "chatbar";
            oscBarWindow.isMoveable = false;


            oscBarCanvas = Instantiate(keyboard.transform.Find("Keyboard Canvas | Manager"),
                oscBarRoot.transform, true).gameObject;
            oscBarCanvas.Rename("KeyboardOSC Bar Canvas");
            oscBarCanvas.GetComponent<Canvas>().referencePixelsPerUnit = 420;
            Destroy(oscBarCanvas.transform.Find("KeyboardToolbar").gameObject);
            Destroy(oscBarCanvas.transform.Find("Keyboard Background/KeyboardLayout").gameObject);
            var kbBackground = oscBarCanvas.transform.Find("Keyboard Background").gameObject;

            foreach (Transform child in kbBackground.transform.Find("KeyboardSettings/Options"))
            {
                if (child.transform.GetSiblingIndex() > 0)
                {
                    Destroy(child.gameObject);
                }
            }

            var oscBarCameraObj = Instantiate(keyboard.GetComponentInChildren<Camera>().gameObject,
                oscBarRoot.transform);
            var oscBarCamera = oscBarCameraObj.GetComponent<Camera>();
            var camTrans = oscBarCamera.transform;
            var canvasTrans = oscBarCanvas.transform;
            oscBarCameraObj.Rename("Camera_OSCBar");

            oscBarCameraObj.DestroyComponent<UI_RescaleCameraToCanvas>();
            var rescaleToCanvas = oscBarCameraObj.AddComponent<RescaleCamToCanvas>();
            rescaleToCanvas.canvas = oscBarCanvas.GetComponent<RectTransform>();
            rescaleToCanvas.camera = oscBarCamera;


            oscBarWindow.renderTexHeightOverride = 60;
            oscBarWindow.renderTexWidthOverride = 730;
            oscBarWindow.widthInMeters = 0.55f;
            oscBarWindow.cameraForTexture = oscBarCamera;
            oscBarWindow.OverlayCanvas = oscBarCanvas.GetComponent<RectTransform>();

            oscBarWindow.canvasGraphicsCaster = oscBarCanvas.GetComponent<GraphicRaycaster>();

            // KEEP or the bar's camera will be borked
            oscBarRoot.transform.position = new Vector3(0.3f, 1, 0);

            var keyboardPos = Overlay_Manager.Instance.Keyboard_Overlay.transform;
            var obwTransform = oscBarWindow.transform;
            obwTransform.position = keyboardPos.TransformDirection(0, 0.01f, 0);
            obwTransform.rotation = new Quaternion(0, 0, 0, 0);

            XSOEventSystem.OnGrabbedOrDroppedOverlay += (targetOverlay, _, grabbed) =>
            {
                if (targetOverlay.overlayKey != "xso.overlay.keyboard" || grabbed) return;
                RepositionBar(oscBarWindow, targetOverlay);
            };

            kbBackground.transform.localPosition = Vector3.zero;
            kbBackground.GetComponent<RectTransform>().sizeDelta = new Vector2(4130, 475);

            camTrans.position = canvasTrans.position;
            camTrans.rotation = canvasTrans.rotation;
            //
            var oscBarTextObj = Instantiate(keyboard.transform
                .Find("Keyboard Canvas | Manager/Keyboard Background/KeyboardSettings/Options/Audio Toggle/Text (TMP)")
                .gameObject, kbBackground.transform);
            Destroy(oscBarCanvas.transform.Find("Keyboard Background/KeyboardSettings").gameObject);
            oscBarTextObj.Rename("KeyboardOSC Bar Text");
            
            var oscbarText = oscBarTextObj.GetComponent<TextMeshProUGUI>();
            XSTools.SetTMPUIText(oscbarText, "type something silly!");
            
            oscbarText.fontSize = 250f;
            oscbarText.fontSizeMax = 250f;
            oscbarText.horizontalAlignment = HorizontalAlignmentOptions.Center;
            oscbarText.verticalAlignment = VerticalAlignmentOptions.Middle;

            
            ChatModeManager.Setup(oscbarText);
            oscBarRoot.SetActive(true);
            keyboardWindowObj.SetActive(true);
            WindowMovementManager.MoveToEdgeOfWindowAndInheritRotation(oscBarWindow, keyboardWindow,
                -0.1f, 0f, 1);
        }

        public void RepositionBar(Unity_Overlay barOverlay, Unity_Overlay keebOverlay)
        {
            WindowMovementManager.ScaleOverlayToScale(keebOverlay.widthInMeters - 0.1f, 0.1f, barOverlay);
            WindowMovementManager.MoveToEdgeOfWindowAndInheritRotation(barOverlay, keebOverlay,
                Vector3.Distance(keebOverlay.transform.position, barOverlay.transform.position) * 0.05f, 0f, 1);
        }

        public void ToggleChatMode()
        {
            ReleaseStickyKeys.Invoke(inputHandler, null);

            IsChatModeActive = !IsChatModeActive;
            oscBarWindowObj.SetActive(IsChatModeActive);

            var barOverlay = oscBarWindowObj.GetComponent<Unity_Overlay>();
            if (IsChatModeActive)
            {
                RepositionBar(barOverlay, overlayManager.Keyboard_Overlay);
            }

            SetToggleColor();
        }

        private void SetToggleColor()
        {
            var chatButton = toggleBarObj.GetComponent<Button>();
            var buttonColors = chatButton.colors;

            buttonColors.normalColor = (IsChatModeActive
                ? UIThemeHandler.Instance.T_HiTone
                : UIThemeHandler.Instance.T_DarkTone);
            chatButton.colors = buttonColors;
        }
    }
}