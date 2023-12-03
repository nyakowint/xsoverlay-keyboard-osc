import { root, Socket, XSOServerMessage, ConnectToServer, SendCommand, DecodeMessage, SetGlobalThemeVariables, Commands } from './common.js';
import * as SettingsElements from './settingsElements.js';
window.Initialize = Initialize;

const ui_canvas = {
    pageRoot: null,
    uiRoot: null,
    sidebar: null,
    sidebar_selector: null,
    mouseglow: null,
}

const sidebar = {
    s_title: null,
    s_steamAvatar: null,
    s_list: null,
}

const sidebarButtons = {
    Keyboard_OSC: null,
    General: null,
    Overlays: null,
    Wrist: null,
    Theme: null,
    Notifications: null,
    Experimental: null,
    Night_Light: null,
    Easter_Eggs: null,
    Support: null,
    Bindings: null,
    Original_Settings: null
}

const sidebarButtonIconNames = [
    "keyboard-fill",
    "gear-fill",
    "badge-vr-fill",
    "clock-fill",
    "palette-fill",
    "bell-fill",
    "box-seam-fill",
    "moon-stars-fill",
    "egg-fill",
    "question-circle-fill",
    "dpad-fill",
    "arrow-left-circle-fill"
];

function UIPage(name, sections, uiRoot) {
    this.Name = name;
    this.Sections = sections;
    this.UiRoot = uiRoot;
}

const SettingsLayout = {
    Original_Settings: {
        _: {
            GoToOgSettings: new SettingsElements.Setting(SettingsElements.UIComponents.Button, 'Go to original Settings', "Press this, then reopen settings if the settings shown here are not up to date yet. You will have to restart XSOverlay to get back here!", '', '', '')
        }
    },
    Keyboard_OSC: {
        _: {
            KBCheckForUpdates: new SettingsElements.Setting(SettingsElements.UIComponents.Toggle, 'Notify about updates', "The plugin will notify you if there's an update available :D", false),
            KBLiveSend: new SettingsElements.Setting(SettingsElements.UIComponents.Toggle, 'Live send mode', "Messages will be sent to the chatbox as you type.", false),
            KBTypingIndicator: new SettingsElements.Setting(SettingsElements.UIComponents.Toggle, 'Send typing indicator', "Some people prefer to not let people know when they are typing for some reason lol", true),
            KBOpenRepo: new SettingsElements.Setting(SettingsElements.UIComponents.Button, 'Plugin Repo', "View this plugin's repo on GitHub", '', '', '')
        },
    },
    General: {
        XSOverlay: {
            AllowAdminPermissions: new SettingsElements.Setting(SettingsElements.UIComponents.Toggle, 'Allow Admin Permissions', "desc", false),
        },
        Accessibility: {
            DominantHand: new SettingsElements.Setting(SettingsElements.UIComponents.Dropdown, 'Dominant Hand', "", 'Right', ['Right', 'Left']),
            Language: new SettingsElements.Setting(SettingsElements.UIComponents.Dropdown, 'Language', "description", 'English', ['English']),
            Crowdin: new SettingsElements.Setting(SettingsElements.UIComponents.Button, 'Crowdin', "", 'box-arrow-up-right', null, 'https://crowdin.com/project/xsoverlay-public'),
        },
        Mouse: {
            InputMethod: new SettingsElements.Setting(SettingsElements.UIComponents.Dropdown, 'Input Method', "", 'MouseEmulation', ['TouchInput', 'MouseEmulation']),
            AutomaticMouseControl: new SettingsElements.Setting(SettingsElements.UIComponents.Toggle, 'Automatic Mouse Control', "", false, null, null, new SettingsElements.ParentSetting('InputMethod', 1)),
            DoubleClickDelay: new SettingsElements.Setting(SettingsElements.UIComponents.Slider, 'Double Click Delay', "", 0.25, [0, 1, 0.01], 'ms', new SettingsElements.ParentSetting('InputMethod', 1)),
            PointerScale: new SettingsElements.Setting(SettingsElements.UIComponents.Slider, 'Pointer Scale', "", 1, [0.25, 1, 0.05], '%')
        },
        User_Interface: {
            HideTooltips: new SettingsElements.Setting(SettingsElements.UIComponents.Toggle, 'Hide Tooltips', "", false),
        },
        Miscellaneous: {
            DiscordRichPresence: new SettingsElements.Setting(SettingsElements.UIComponents.Toggle, 'Discord Rich Presence', "", true),
            LowBatterySound: new SettingsElements.Setting(SettingsElements.UIComponents.Toggle, 'Low Battery Sound', "", true),
            LowBatteryWarningPercent: new SettingsElements.Setting(SettingsElements.UIComponents.Slider, 'Low Battery Warning Percent', "", 15, [5, 25, 1], '%'),
            HapticsStrength: new SettingsElements.Setting(SettingsElements.UIComponents.Slider, 'Haptic Feedback', "", 1, [0, 1, 0.01], '%')
        },
        Analytics: {
            SendAnalytics: new SettingsElements.Setting(SettingsElements.UIComponents.Toggle, 'Send Analytics',
                "Data sent is completely anonymous and not tied to you as an individual, and is not considered as \"personally identifiable.\" ; Data collected goes towards making the software better. <br><br>Data includes:<br> - System Specs<br> - HMD Model<br> - Changed Settings<br> - Error / Crash Reporting<br> - VR Application<br> - Startup / Shutdown",
                true),
        },
        _: {
            VersionNumber: new SettingsElements.Setting(SettingsElements.UIComponents.Text, 'VersionNumber'),
        },
    },
    Overlays: {
        Capture: {
            CaptureMethod: new SettingsElements.Setting(SettingsElements.UIComponents.Dropdown, 'Window Capture API', "", 'Windows Graphics Capture', ['WindowsGraphicsCapture', 'BitBlt']),
            // ShowWindowPreviews: new SettingsElements.Setting(SettingsElements.UIComponents.Toggle, 'Show Window Thumbnails', true) NOTE:: DEPRECATED
        },
        Overlay_Behavior: {
            AutoRecenter: new SettingsElements.Setting(SettingsElements.UIComponents.Toggle, 'Auto Recenter', "", true),
            InvertScaleGesture: new SettingsElements.Setting(SettingsElements.UIComponents.Toggle, 'Invert Scale Gesture', "", false),
        },
        Overlay_Display: {
            CurvedOverlays: new SettingsElements.Setting(SettingsElements.UIComponents.Toggle, 'Curved Overlays', "", true),
            OverlayCurveBias: new SettingsElements.Setting(SettingsElements.UIComponents.Slider, 'Overlay Curve Bias', "", 1.5, [0.1, 5, 0.01], null, new SettingsElements.ParentSetting('CurvedOverlays', true)),
            OverlayClipAngle: new SettingsElements.Setting(SettingsElements.UIComponents.Slider, 'Overlay Clip Angle', "", 68, [0, 180, 1], '°'),
            OverlayDefaultScale: new SettingsElements.Setting(SettingsElements.UIComponents.Slider, 'Default Scale', "", 1, [0, 1, 0.01], '%'),
            OverlayDefaultOpacity: new SettingsElements.Setting(SettingsElements.UIComponents.Slider, 'Default Opacity', "", 1, [0, 1, 0.01], '%'),
            OverlayDefaultMinFPS: new SettingsElements.Setting(SettingsElements.UIComponents.Slider, 'Default Min FPS', "", 30, [1, 144, 1], 'FPS'),
            OverlayDefaultMaxFPS: new SettingsElements.Setting(SettingsElements.UIComponents.Slider, 'Default Max FPS', "", 60, [1, 144, 1], 'FPS'),
        },
        Overlay_Movement_and_Handling: {
            AimTarget: new SettingsElements.Setting(SettingsElements.UIComponents.Dropdown, 'Target', "", 'Mixed', ['Mixed', 'HMD', 'Controller']),
            AimAtTarget: new SettingsElements.Setting(SettingsElements.UIComponents.Toggle, 'Aim At Target', "", true),
            GrabSensitivity: new SettingsElements.Setting(SettingsElements.UIComponents.Slider, 'Grab Sensitivity', "", 0.3, [0, 1, 0.01], '%'),
            PushSpeed: new SettingsElements.Setting(SettingsElements.UIComponents.Slider, 'Push Pull Speed', "", 0.03, [0, 0.2, 0.01]),
            ScaleSpeed: new SettingsElements.Setting(SettingsElements.UIComponents.Slider, 'Scale Speed', "", 3, [1, 6, 0.01]),
            PositionDampening: new SettingsElements.Setting(SettingsElements.UIComponents.Slider, 'Position Dampening', "", 15, [1, 100, 1]),
            RotationDampening: new SettingsElements.Setting(SettingsElements.UIComponents.Slider, 'Rotation Dampening', "", 15, [1, 100, 1]),
            SmartRollAngle: new SettingsElements.Setting(SettingsElements.UIComponents.Slider, 'Smart Roll Angle', "", 50, [10, 180, 1], '°')
        },
    },
    Wrist: {
        Wrist_Overlay: {
            AllowWristMovement: new SettingsElements.Setting(SettingsElements.UIComponents.Toggle, 'Allow Movement', "", true),
            HideWristOverlay: new SettingsElements.Setting(SettingsElements.UIComponents.Toggle, 'Hide Wrist Overlay', "", false),
            WristOpacity: new SettingsElements.Setting(SettingsElements.UIComponents.Slider, 'Opacity', "", 1, [0.1, 1, 0.01], '%'),
            WristClipAngle: new SettingsElements.Setting(SettingsElements.UIComponents.Slider, 'Clip Angle', "", 60, [10, 180, 1], '°'),
            ResetWristPosition: new SettingsElements.Setting(SettingsElements.UIComponents.Button, 'Reset Wrist Position', "", '', '', ''),
        },
        Date_and_Time:
        {
            ContinentalTimeFormat: new SettingsElements.Setting(SettingsElements.UIComponents.Toggle, '24 Hour Clock', "", false),
            DateFormat: new SettingsElements.Setting(SettingsElements.UIComponents.Dropdown, 'Date Format', "", 'DD / MM / YY', ['DD / MM / YY', 'MM / DD / YY', 'YY / MM / DD'])
        },
        Battery_Widget: {
            AlwaysShowDetailedInformation: new SettingsElements.Setting(SettingsElements.UIComponents.Toggle, 'Always Show Details', "", false),
            DefaultShowBatteryPercentage: new SettingsElements.Setting(SettingsElements.UIComponents.Toggle, 'Show Percentage by Default', "", true),
            BatteryFontScale: new SettingsElements.Setting(SettingsElements.UIComponents.Slider, 'Battery Font Size', "", 16, [12, 24, 1], 'px')
        },
        Media_Player: {
            AutoMediaDetection: new SettingsElements.Setting(SettingsElements.UIComponents.Toggle, 'Smart Media Controls', "", false),
        }
    },
    Theme: {
        Base_Theme: {
            UseDarkTheme: new SettingsElements.Setting(SettingsElements.UIComponents.Toggle, 'Dark Mode', "", true),
            AdaptiveColor: new SettingsElements.Setting(SettingsElements.UIComponents.Slider, 'Adaptive Color Strength', "", 0.1, [0, 1, 0.01], '%'),
            MediaThemeing: new SettingsElements.Setting(SettingsElements.UIComponents.Toggle, 'Media Themeing', "", false),
        },
        Accent_Color: {
            AccentColorR: new SettingsElements.Setting(SettingsElements.UIComponents.Slider, 'Red Amount', "", 0.2, [0, 1, 0.01], '%'),
            AccentColorG: new SettingsElements.Setting(SettingsElements.UIComponents.Slider, 'Green Amount', "", 0.82, [0, 1, 0.01], '%'),
            AccentColorB: new SettingsElements.Setting(SettingsElements.UIComponents.Slider, 'Blue Amount', "", 0.4, [0, 1, 0.01], '%'),
        },
    },
    Notifications: {
        Appearance: {
            NotificationScale: new SettingsElements.Setting(SettingsElements.UIComponents.Slider, 'Scale', "", 0.25, [0.1, 2, 0.001]),
            NotificationPositionX: new SettingsElements.Setting(SettingsElements.UIComponents.Slider, 'Position X', "", 0, [-2, 2, 0.001]),
            NotificationPositionY: new SettingsElements.Setting(SettingsElements.UIComponents.Slider, 'Position Y', "", -0.125, [-2, 2, 0.001]),
            NotificationPositionZ: new SettingsElements.Setting(SettingsElements.UIComponents.Slider, 'Position Z', "", 0.55, [-2, 2, 0.001]),
            SmallNotificationTest: new SettingsElements.Setting(SettingsElements.UIComponents.Button, 'Test Small', "", '', '', ''),
            LargeNotificationTest: new SettingsElements.Setting(SettingsElements.UIComponents.Button, 'Test Large', "", '', '', ''),
        }
    },
    Experimental: {
        Experimental_Settings: {
            ForceHigherQualityOverlays: new SettingsElements.Setting(SettingsElements.UIComponents.Toggle, 'Force High Quality Overlays', "Forces SteamVR to scale the compositor layer to 125% render scale. <br>This requires a restart of SteamVR.", false),
            BlockInputToBackgroundApplication: new SettingsElements.Setting(SettingsElements.UIComponents.Toggle, 'Block Inputs To Background App', "Blocks all input to background applications when focusing any overlay.", false),
            InputBlockingBehavior: new SettingsElements.Setting(SettingsElements.UIComponents.Dropdown, 'Blocking Behavior', "", 'LayoutMode', ['Hover', 'LayoutMode'], null, new SettingsElements.ParentSetting('BlockInputToBackgroundApplication', true)),
        }
    },
    Night_Light: {
        _: {
            NightLight: new SettingsElements.Setting(SettingsElements.UIComponents.Toggle, 'Enable Night Light', "", false),
            NightLightIntensity: new SettingsElements.Setting(SettingsElements.UIComponents.Slider, 'Intensity', "", 0.5, [0, 1, 0.01], '%')
        },
        Schedule: {
            NightLightSchedule: new SettingsElements.Setting(SettingsElements.UIComponents.Toggle, 'Use Schedule', "", false),
            NightLightStartHour: new SettingsElements.Setting(SettingsElements.UIComponents.Slider, 'Start Time', "", 0.5, [1, 12, 1], null, new SettingsElements.ParentSetting('NightLightSchedule', true)),
            NightLightStartAMPM: new SettingsElements.Setting(SettingsElements.UIComponents.Toggle, 'AM - PM', "", null, null, null, new SettingsElements.ParentSetting('NightLightSchedule', true)),
            NightLightEndHour: new SettingsElements.Setting(SettingsElements.UIComponents.Slider, 'End Time', "", 0.5, [1, 12, 1], null, new SettingsElements.ParentSetting('NightLightSchedule', true)),
            NightLightEndAMPM: new SettingsElements.Setting(SettingsElements.UIComponents.Toggle, 'AM - PM', "", null, null, null, new SettingsElements.ParentSetting('NightLightSchedule', true)),
        }
    },
    Easter_Eggs: {
        Why_Are_They_Called_Easter_Eggs: {
            EasterEggBee: new SettingsElements.Setting(SettingsElements.UIComponents.Toggle, 'Beeeeeeeeeeeeeeeee!!!!', "", false, null, null, null, new SettingsElements.ParentAchievement('SUMMONBEE', true))
        },
    },
    Support: {
        Support_Links: {
            Documentation: new SettingsElements.Setting(SettingsElements.UIComponents.Button, 'Docs', "", 'xsoverlay', 'styled', 'https://xiexe.github.io/XSOverlayDocumentation/#/'),
            Discord: new SettingsElements.Setting(SettingsElements.UIComponents.Button, 'Discord', "", 'discord', 'styled', 'https://discord.gg/AV4V5hB'),
            Twitter: new SettingsElements.Setting(SettingsElements.UIComponents.Button, 'Twitter', "", 'twitter', 'styled', 'https://twitter.com/XiexeVR'),
            Github: new SettingsElements.Setting(SettingsElements.UIComponents.Button, 'Github', "", 'github', 'styled', 'https://github.com/Xiexe/XSOverlay-Issue-Tracker/issues/new/choose'),
            Steam: new SettingsElements.Setting(SettingsElements.UIComponents.Button, 'Steam', "", 'steam', 'styled', 'https://steamcommunity.com/app/1173510/discussions/'),
        }
    },
    Bindings: {
        _: {
            Bindings: new SettingsElements.Setting(SettingsElements.UIComponents.Text, 'Bindings', "description"),
        }
    }
}

const Settings = {
    UiPages: []
};

function Initialize(name) {
    
    ui_canvas.pageRoot = document.getElementById('uiContainer');
    ui_canvas.uiRoot = SettingsElements.InstantiateUiElement(ui_canvas.pageRoot, SettingsElements.UITypes.div, ['background', 'theme-dark']);
    ui_canvas.pageContainer = SettingsElements.InstantiateUiElement(ui_canvas.uiRoot, SettingsElements.UITypes.div, ['page-wrapper', 'absolute']);
    InitializeUI();
    ConnectToServer(name);
    SetupSocketListeners();
}

function InitializeUI() {
    PopulateSidebar();
    CreateUIPages();
    OnSwitchPage('General');
}

function PopulateSidebar() {
    ui_canvas.sidebar = SettingsElements.InstantiateUiElement(ui_canvas.uiRoot, SettingsElements.UITypes.div, ['side-bar', 'theme-mid', 'absolute']);
    ui_canvas.sidebar_selector = SettingsElements.InstantiateUiElement(ui_canvas.sidebar, SettingsElements.UITypes.div, ['side-bar-selector', 'absolute']);
    sidebar.s_list = SettingsElements.InstantiateUiElement(ui_canvas.sidebar, SettingsElements.UITypes.div, ['side-bar-button-container']);

    var i = 0;
    Object.keys(sidebarButtons).forEach(function (key) {
        sidebarButtons[key] = SettingsElements.InstantiateUiElement(sidebar.s_list, SettingsElements.UITypes.button, ['side-bar-button']);

        sidebarButtons[key].addEventListener("click", function (e) {
            setTimeout(function () { sidebarButtons[key].blur(); }, 150); //deselect button
            OnSwitchPage(key);
            e.preventDefault;
        });


        var icon = SettingsElements.InstantiateUiElement(sidebarButtons[key], SettingsElements.UITypes.img, ['side-bar-button-icon', 'theme-font-contrast']);
        icon.classList.add(`bi-${sidebarButtonIconNames[i]}`);

        var text = SettingsElements.InstantiateUiElement(sidebarButtons[key], SettingsElements.UITypes.div, ['side-bar-button-text']);
        text.innerHTML = key.replaceAll('_', ' ');
        text.setAttribute('translationKey', key);

        i++;

        if (i < Object.keys(sidebarButtons).length)
            SettingsElements.Divider(sidebar.s_list, 'sidebar-divider');
    });
}

function CreateUIPages() {
    for (var pageLayout in SettingsLayout) {
        var uiRoot = SettingsElements.InstantiateUiElement(ui_canvas.pageContainer, SettingsElements.UITypes.div, ['page-container', 'theme-dark']);
        uiRoot.setAttribute('id', `Page_${pageLayout}`);

        var pageHeader = SettingsElements.InstantiateUiElement(uiRoot, SettingsElements.UITypes.div, ['page-header']);
        var pageHeaderText = SettingsElements.InstantiateUiElement(pageHeader, SettingsElements.UITypes.div, ['page-header-text']);
        pageHeaderText.innerHTML = `${pageLayout.replaceAll('_', ' ')}`
        pageHeaderText.setAttribute('translationKey', pageLayout);

        if (pageLayout == 'Theme') {
            SettingsElements.InstantiateUiElement(uiRoot, SettingsElements.UITypes.div, ['background-splash']);
            uiRoot.classList.replace('page-container', 'page-container-no-overflow')
        }

        var sections = [];
        for (var sectionLayout in SettingsLayout[pageLayout]) {
            var settingsLayout = SettingsLayout[pageLayout][sectionLayout];
            var numOfSettings = Object.keys(settingsLayout).length;
            var sectionTitle = sectionLayout;

            var createdSection = new SettingsElements.Section(sectionTitle, numOfSettings, uiRoot);
            sections.push(createdSection);

            var index = 0;
            for (var setting in settingsLayout) {
                var settingDefinition = SettingsLayout[pageLayout][sectionLayout][setting];
                var name = settingDefinition.displayName;
                var type = settingDefinition.type;
                var defaultState = settingDefinition.defaultState;
                var opts = settingDefinition.opts;
                var opts1 = settingDefinition.opts1;
                var desc = settingDefinition.description;
                settingDefinition.internalName = setting;
                settingDefinition.sectionID = sectionTitle;

                switch (type) {
                    case SettingsElements.UIComponents.Text:
                        SettingsElements.Description(createdSection.Background, desc, `${settingDefinition.internalName}_Desc`);
                        break;

                    case SettingsElements.UIComponents.Button:
                        SettingsElements.Button(settingDefinition, createdSection.Background);
                        break;

                    case SettingsElements.UIComponents.Toggle:
                        SettingsElements.Toggle(settingDefinition, name, defaultState, opts, createdSection.Background);
                        break;

                    case SettingsElements.UIComponents.Slider:
                        SettingsElements.Slider(settingDefinition, name, defaultState, opts, opts1, createdSection.Background, 300);
                        break;

                    case SettingsElements.UIComponents.Dropdown:
                        SettingsElements.Dropdown(settingDefinition, name, defaultState, opts, createdSection.Background, 300);
                        break;
                }

                if (desc != "" && type != SettingsElements.UIComponents.Text) {
                    SettingsElements.Description(createdSection.Background, desc, `${settingDefinition.internalName}_Desc`);
                }

                index++;
                if (index < Object.keys(settingsLayout).length) {
                    SettingsElements.Divider(createdSection.Background, 'divider', setting);
                }
            }
        }

        var page = new UIPage(pageLayout, sections, uiRoot);
        Settings.UiPages.push(page);
    }
}

function OnSwitchPage(pageName) {
    ui_canvas.sidebar.classList.remove('side-bar-hoverable');
    setTimeout(function () {
        for (var page in Settings.UiPages) {
            if (Settings.UiPages[page].Name != pageName) {
                Settings.UiPages[page].UiRoot.style.animation = '0.3s ease fade-out forwards';
            }
            else {
                Settings.UiPages[page].UiRoot.style.animation = '0.3s ease fade-in forwards';
            }
        }

        for (var tab in sidebarButtons) {
            if (tab == pageName) {
                sidebarButtons[tab].classList.add('side-bar-button-selected');
                sidebarButtons[tab].firstElementChild.classList.add('selected-icon');
            }
            else {
                sidebarButtons[tab].classList.remove('side-bar-button-selected');
                sidebarButtons[tab].firstElementChild.classList.remove('selected-icon');
            }
        }

        setTimeout(function () {
            ui_canvas.sidebar.classList.add('side-bar-hoverable');
        }, 300);
    }, 200);

    if (pageName == "Bindings") {
        SendCommand(Commands.OpenSteamVRBindings, null, null);
    }
}



// Message Handling Functions
function SetupSocketListeners() {
    Socket.CurrentSocket.addEventListener('open', () => {
        SendCommand(Commands.RequestThemeUpdate, null, 'subscribe');
        SendCommand(Commands.RequestUpdatedLanguageList, null, 'subscribe');
        SendCommand(Commands.RequestUpdatedLanguageMap, null, 'subscribe');
        SendCommand(Commands.RequestGetSettings, null, null);
        SendCommand(Commands.RequestAchievementInformation, null, null);
    });

    Socket.CurrentSocket.addEventListener('message', function message(data) {
        HandleMessages(data);
    });

    Socket.CurrentSocket.addEventListener('close', () => {
        console.log(`${XSOServerMessage.sender} websocket was disconnected. Attempting reconnect.`);
        setTimeout(function () {
            ConnectToServer('settings');
            SetupSocketListeners();
            console.log("Reconnecting...");
        }, 1000);
    });
}


function HandleMessages(msg) {
    var decoded = DecodeMessage(msg);
    switch (decoded.Command) {
        case 'UpdateSteamAvatar':
            // Wrist.ProfileImage.src = `data:image/png;base64,${decoded.RawData}`
            break;

        case 'UpdateTheme':
            SetGlobalThemeVariables(decoded.JsonData);
            break;

        case 'UpdateSettings':
            SetMenuStates(decoded.JsonData);
            break;

        case 'UpdateLanguageList':
            UpdateLanguageList(decoded.JsonData);
            break;

        case 'UpdateLanguageMap':
            UpdateLocalizedTextElements(decoded.JsonData);
            break;

        case 'UpdateAchievementStatus':
            SettingsElements.CheckForSettingsThatRelyOnSteamAchievements(decoded.JsonData);
            break;
    }
}

function UpdateLanguageList(data) {
    var languages = Object.keys(data);

    var languageNames = [];
    var languageURLs = [];
    for (var i in languages) {
        languageNames.push(languages[i]);
        languageURLs.push(data[languages[i]]);
    }

    SettingsLayout.General.Accessibility.Language.opts = languageNames;
    SettingsLayout.General.Accessibility.Language.opts1 = languageURLs;
    SettingsElements.DropdownRepopulate(SettingsLayout.General.Accessibility.Language);
}

function UpdateLocalizedTextElements(languageMap) {
    var translationKeys = Object.keys(languageMap);

    for (var key in translationKeys) {
        var translateableText = document.querySelectorAll(`[translationKey="${translationKeys[key]}"]`);

        if (translateableText.length <= 0)
            continue;

        for (var i in translateableText) {
            var textElement = translateableText[i];
            if (textElement.innerHTML == null)
                continue;

            if (textElement.id == 'ApplicationVersion_Desc')
                continue;

            // console.log(`Updating Text Element: ${translationKeys[key]}`);
            textElement.innerHTML = languageMap[translationKeys[key]];
        }
    }
}

function SetMenuStates(data) {
    for (var key in data) {
        var value = data[key];
        if (key == 'AccentColorR' || key == 'AccentColorG' || key == 'AccentColorB') // Need special handling for vectors...
            continue;

        if (key == 'AccentColor') {
            var sliderR = document.getElementById('AccentColorR');
            var sliderG = document.getElementById('AccentColorG');
            var sliderB = document.getElementById('AccentColorB');

            SettingsElements.UpdateSliderUI(sliderR, value.r);
            SettingsElements.UpdateSliderUI(sliderG, value.g);
            SettingsElements.UpdateSliderUI(sliderB, value.b);
        }
        else if (key == 'NotificationOffsets') {
            var sliderX = document.getElementById('NotificationPositionX');
            var sliderY = document.getElementById('NotificationPositionY');
            var sliderZ = document.getElementById('NotificationPositionZ');

            SettingsElements.UpdateSliderUI(sliderX, value.x);
            SettingsElements.UpdateSliderUI(sliderY, value.y);
            SettingsElements.UpdateSliderUI(sliderZ, value.z);
        }
        else {
            var uiElement = document.getElementById(key);
            if (uiElement != null) {
                switch (uiElement.getAttribute('uiType')) {
                    case 'toggle':
                        uiElement.checked = value;
                        break;

                    case 'slider':
                        SettingsElements.UpdateSliderUI(uiElement, value);
                        break;

                    //TODO:: Dropdowns can get desynced from what is actually selected if they're dynamically populated.
                    case 'dropdown':
                        var options = uiElement.querySelectorAll('.selectopt');
                        for (var option in options) {
                            var opt = options[option];
                            if (opt.id == null)
                                continue;

                            if (opt.id.match(value)) {
                                opt.checked = true;
                                break;
                            }
                        }
                        break;
                }

                SettingsElements.CheckForSettingsThatRelyOnChangedSetting(key, value);
            }
        }
    }

    var appVersionText = document.getElementById("VersionNumber_Desc");
    appVersionText.innerHTML = `Build ${data.VersionNumber}`;

    console.log("Settings Updated.");
}