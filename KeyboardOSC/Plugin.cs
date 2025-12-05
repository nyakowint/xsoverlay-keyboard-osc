using System;
using System.Reflection;
using System.Threading.Tasks;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using KeyboardOSC;
using KeyboardOSC.XScripts;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Vuplex.WebView;
using XSOverlay;
using XSOverlay.WebApp;

[assembly: AssemblyVersion("1.2.7")]

namespace KeyboardOSC
{
    [BepInPlugin("nwnt.keyboardosc", "KeyboardOSC", PluginVersion)]
    public class Plugin : BaseUnityPlugin
    {
        public const string PluginVersion = "1.2.7";
        public static Plugin Instance;
        public static ManualLogSource PluginLogger;

        public static bool IsDebugConfig = false;
        public static bool IsDevBuild;
        public static bool ChatModeActive;
        public static bool ModifiedUiSuccess;

        public Overlay_Manager overlayManager;
        public KeyboardInputHandler inputHandler;

        public GameObject sidebarBtnObj;
        public GameObject oscBarWindowObj;
        public GameObject oscBarCanvas;

        public static MethodInfo ReleaseStickyKeys;
        
        private bool _hasInitialized = false;
        private int _initAttempts = 0;

        private void Awake()
        {
#if DEBUG
            IsDebugConfig = true;
#elif DEV
            Logger.LogWarning("YOU ARE USING A DEVELOPMENT BUILD AND THINGS MAY NOT WORK RIGHT!!"); 
            Logger.LogWarning("if ur me then go main for vrchillin u dork");
            // yes this is necessary otherwise i will just ignore it
            Logger.LogWarning("!! DEVELOPMENT BUILD !! ");
            Logger.LogWarning("!! DEVELOPMENT BUILD !! ");
            Logger.LogWarning("!! DEVELOPMENT BUILD !! ");
            Logger.LogWarning("!! DEVELOPMENT BUILD !! ");
            Logger.LogWarning("!! DEVELOPMENT BUILD !! ");
            Logger.LogWarning("!! DEVELOPMENT BUILD !! ");
            Logger.LogWarning("!! DEVELOPMENT BUILD !! ");
            Logger.LogWarning("!! DEVELOPMENT BUILD !! ");
                IsDebugConfig = true;
                IsDevBuild = true;
#endif
            PluginLogger = Logger;
            if (Instance) Destroy(this);
            PluginSettings.ConfigFile = Config;
            PluginSettings.Init();
            
            ModifiedUiSuccess = Tools.DownloadModifiedUi();

            if (!Environment.CommandLine.Contains("-batchmode") || IsDebugConfig) return;
            Logger.LogWarning("XSOverlay runs in batchmode normally (headless without a window).");
            Logger.LogWarning("To see extended logs launch XSOverlay directly.");
        }

        private void Start()
        {
            Instance = this;
            Logger.LogInfo($"Keyboard OSC v{PluginVersion} started!");
            Logger.LogWarning("Report plugin-specific issues to the GitHub repo.");
            Logger.LogWarning("!! / Please remove KeyboardChatbox before reporting bugs to XSOverlay developers! \\ !!");
            Console.Title = "KeyboardOSC - XSOverlay";

            ReleaseStickyKeys = Tools.SafeMethod(typeof(KeyboardInputHandler), "ReleaseStickyKeys");
            Patches.PatchAll();

            ServerClientBridge.Instance.Api.Commands["Keyboard"] = delegate
            {
                InitializeKeyboard();
                Overlay_Manager.Instance.EnableKeyboard();
            };
        }

        public void InitializeKeyboard()
        {
            _initAttempts++;
            
            // Failsafe: If this is being called again, something went wrong
            if (_hasInitialized)
            {
                Logger.LogError($"[FAILSAFE] InitializeKeyboard called again (attempt #{_initAttempts})! Previous initialization may have failed silently.");
                Logger.LogWarning("[FAILSAFE] Falling back to default keyboard behavior without KeyboardOSC features.");
                // Don't try to initialize again, just let the keyboard open normally
                return;
            }
            
            try
            {
                // Plugin startup logic
                Logger.LogInfo("[Stage 0] Keyboard setting up-");
                overlayManager = Overlay_Manager.Instance;
                inputHandler = overlayManager.Keyboard.GetComponent<KeyboardInputHandler>();

                SetupToggleButton();
                SetupBar();

                ServerClientBridge.Instance.Api.Commands["Keyboard"] = delegate
                {
                    Overlay_Manager.Instance.EnableKeyboard();
                };

                var checkUpdates = PluginSettings.GetSetting<bool>("CheckForUpdates");
                if (checkUpdates.Value) Task.Run(Tools.CheckVersion);
                
                // Mark as successfully initialized
                _hasInitialized = true;
                Logger.LogInfo("[Stage 3] Keyboard setup complete!");
            }
            catch (Exception ex)
            {
                Logger.LogError($"[CRITICAL] Failed to initialize KeyboardOSC: {ex.Message}");
                Logger.LogError($"Stack trace: {ex.StackTrace}");
                Logger.LogWarning("[FAILSAFE] KeyboardOSC features will be disabled. Keyboard will work in default mode.");
                
                // Reset the command to fallback behavior
                ServerClientBridge.Instance.Api.Commands["Keyboard"] = delegate
                {
                    Overlay_Manager.Instance.EnableKeyboard();
                };
            }
        }

        private void SetupToggleButton()
        {
            Logger.LogInfo("[Stage 1] Setup toggle button");
            var lockButtonObj = overlayManager.Keyboard.GetComponentInChildren<LockKeyboardButton>(true).gameObject;
            sidebarBtnObj = Instantiate(lockButtonObj, lockButtonObj.transform.parent);
            sidebarBtnObj.DestroyComponent<LockKeyboardButton>();
            sidebarBtnObj.AddComponent<ToggleChatButton>();
            sidebarBtnObj.transform.Find("Image").GetComponent<Image>().sprite = "chat".GetSprite();
            sidebarBtnObj.Rename("KeyboardOSC Toggle");
        }

        private void SetupBar()
        {
            Logger.LogInfo("[Stage 2] Setup keyboard island (bar)");
            var keyboard = overlayManager.Keyboard;
            var keyboardWindow = overlayManager.Keyboard_Overlay;
            var keyboardWindowObj = overlayManager.Keyboard_Overlay.gameObject;
            keyboardWindowObj.SetActive(false);


            // // TODO: TEST
            //
            // Logger.LogWarning("pat setup");
            // Logger.LogWarning(overlayManager.GlobalSettingsMenuOverlay.gameObject.name);
            // Logger.LogWarning(overlayManager.GlobalSettingsMenuOverlay.gameObject.transform.parent.name);
            // var patWindow = Instantiate(overlayManager.GlobalSettingsMenuOverlay.gameObject,
            //     overlayManager.GlobalSettingsMenuOverlay.transform.parent);
            // Destroy(patWindow.GetComponent<OverlayWebView>());
            // Destroy(patWindow.transform.GetChild(0).gameObject);
            //
            // patWindow.AddComponent<OverlayTopLevelObject>();
            // // https://developer.vuplex.com/webview/WebViewPrefab
            // var webView = patWindow.AddComponent<OverlayWebView>();
            // webView.UserInterfaceSelection = OverlayWebView.UserInterfacePaths.URL;
            // webView.URL = "https://google.com";
            //
            // var patOverlay = patWindow.GetComponent<Unity_Overlay>();
            // patOverlay.overlayName = "kbosc";
            //
            //
            // patOverlay.overlayRootObject = oscBarWindowObj;
            // patWindow.GetComponent<OverlayIdentifier>().OverlayTopLevelObject = oscBarWindowObj;
            //
            // SetupWebView(webView, patWindow, patOverlay);
            //
            // // TODO: END TEST

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

            var kbOpacityField = Tools.SafeField(typeof(XSettingsManager), "KeyboardOpacity");
            if (kbOpacityField != null)
            {
                var kbOpacity = (Slider)kbOpacityField.GetValue(XSettingsManager.Instance);
                if (kbOpacity != null)
                {
                    kbOpacity.onValueChanged.AddListener(value => { oscBarWindow.opacity = value; });
                }
                else
                {
                    Logger.LogWarning("KeyboardOpacity slider instance was null; opacity sync disabled.");
                }
            }
            else
            {
                Logger.LogWarning("KeyboardOpacity field not found; bar opacity will not sync.");
            }

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
            var oscBarTextObj =
                Instantiate(
                    keyboard.transform
                        .Find(
                            "Keyboard Canvas | Manager/Keyboard Background/KeyboardSettings/Options/Audio Toggle/Text (TMP)")
                        .gameObject, kbBackground.transform);
            var barCharCounter = Instantiate(oscBarTextObj, kbBackground.transform);
            Destroy(oscBarCanvas.transform.Find("Keyboard Background/KeyboardSettings").gameObject);
            oscBarTextObj.Rename("KeyboardOSC Bar Text");
            barCharCounter.Rename("Character Counter");

            var oscbarText = oscBarTextObj.GetComponent<TextMeshProUGUI>();
            var charCounterText = barCharCounter.GetComponent<TextMeshProUGUI>();
            XSTools.SetTMPUIText(oscbarText, "type something silly!");
            XSTools.SetTMPUIText(charCounterText, "0/144");

            oscbarText.fontSize = 250f;
            oscbarText.fontSizeMax = 250f;
            oscbarText.horizontalAlignment = HorizontalAlignmentOptions.Center;
            oscbarText.verticalAlignment = VerticalAlignmentOptions.Middle;

            charCounterText.fontSize = 100f;
            charCounterText.fontSizeMax = 100f;
            charCounterText.fontStyle = FontStyles.Bold;
            charCounterText.horizontalAlignment = HorizontalAlignmentOptions.Right;
            charCounterText.verticalAlignment = VerticalAlignmentOptions.Top;


            ChatMode.Setup(oscbarText, charCounterText);

            var trackDevice = keyboardWindowObj.AddComponent<Overlay_TrackDevice>();
            trackDevice.TrackedDevice = Unity_Overlay.OverlayTrackedDevice.None;

            oscBarRoot.SetActive(true);
            keyboardWindowObj.SetActive(true);
            WindowMovementManager.MoveToEdgeOfWindowAndInheritRotation(oscBarWindow, keyboardWindow,
                -0.1f, 0f, 1);
        }


        private async void SetupWebView(OverlayWebView overlayWebView, GameObject patWindow, Unity_Overlay overlay)
        {
            var num = overlayWebView.pixelDensity * 1300f;
            var webView = WebViewPrefab.Instantiate(overlayWebView.Width / num, overlayWebView.Height / num);
            webView.PixelDensity = overlayWebView.pixelDensity;
            webView.transform.parent = patWindow.transform;
            
            // awake isnt doing its job so ill just do it myself smh
            var webviewSetter = typeof(OverlayWebView).GetProperty("_webView");
            webviewSetter.DeclaringType.GetProperty("_webView");
            webviewSetter.SetValue(overlayWebView, webView,
                BindingFlags.NonPublic | BindingFlags.Instance, null, null, null);
            await webView.WaitUntilInitialized();
            overlay.WebViewHandler = webView;
            overlay.OverlayWebView = overlayWebView;
            overlay.SetTextureBoundsToInvertedBounds();
            overlay.overlayTexture = webView.WebView.Texture;
            overlay.renderTexHeightOverride = (int)overlayWebView.Height;
            overlay.renderTexWidthOverride = (int)overlayWebView.Width;
            webView.LogConsoleMessages = overlayWebView.enableLogging;
            webView.RemoteDebuggingEnabled = overlayWebView.enableDebugging;
            webView.WebView.LoadUrl(overlayWebView.URL);
            webView.GetComponentInChildren<Collider>().gameObject.SetActive(value: false);
            XSOEventSystem.UpdateWebviewCollision += overlayWebView.UpdateCollisionTexture;
            XSOEventSystem.OnSwitchHoveringOverlay += (_, uo) =>
            {
                if (uo == overlay) overlayWebView.UpdateCollisionTexture(overlayWebView);
            };
            webView.WebView.LoadProgressChanged += (sender, args) =>
            {
                if (overlayWebView.ReparentTarget != null)
                {
                    overlayWebView.transform.SetParent(overlayWebView.ReparentTarget);
                }

                if (!overlayWebView.DisableOnStart) return;
                overlayWebView._overlay.overlayRootObject.SetActive(value: false);
                overlayWebView.gameObject.SetActive(value: false);
            };
            /*webView.WebView.FocusChanged += overlayWebView.HandleWebViewFocusChanged;*/
            /*XSOEventSystem.Current.EventRegisterWebviewOverlay(overlayWebView);*/
        }

        public void RepositionBar(Unity_Overlay barOverlay, Unity_Overlay keebOverlay)
        {
            barOverlay.opacity = keebOverlay.opacity;
            WindowMovementManager.ScaleOverlayToScale(keebOverlay.widthInMeters - 0.1f, 0.1f, barOverlay);
            WindowMovementManager.MoveToEdgeOfWindowAndInheritRotation(barOverlay, keebOverlay,
                Vector3.Distance(keebOverlay.transform.position, barOverlay.transform.position) * 0.05f, 0f, 1);
        }

        public void AttachKeyboard(int anchor)
        {
            WindowMovementManager.WMM_Inst.AttachWindowToDeviceIndex(anchor, Overlay_Manager.Instance.Keyboard_Overlay);
        }

        public void ToggleChatMode()
        {
            ReleaseStickyKeys?.Invoke(inputHandler, null);

            ChatModeActive = !ChatModeActive;
            oscBarWindowObj.SetActive(ChatModeActive);

            var barOverlay = oscBarWindowObj.GetComponent<Unity_Overlay>();
            if (ChatModeActive)
            {
                RepositionBar(barOverlay, overlayManager.Keyboard_Overlay);
            }

            SetToggleColor();
        }

        private void SetToggleColor()
        {
            var chatButton = sidebarBtnObj.GetComponent<Button>();
            var buttonColors = chatButton.colors;

            buttonColors.normalColor = (ChatModeActive
                ? UIThemeHandler.Instance.T_HiTone
                : UIThemeHandler.Instance.T_DarkTone);
            chatButton.colors = buttonColors;
        }
    }
}
