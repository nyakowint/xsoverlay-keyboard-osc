import * as Api from '../../../../_Shared/api.js';
import * as OpenVR from '../../../../_Shared/openvrapi.js';
import * as Common from '../js/common.js';
import * as Ui from '../js/uiComponents.js';
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
            GoToOgSettings: new Ui.Setting(Ui.ComponentType.Button, 'Go to original Settings', "Press this, then reopen settings if the settings shown here are not up to date yet. You will have to restart XSOverlay to get back here!", null, null, null)
        }
    },
    Keyboard_OSC: {
        _: {
            KBCheckForUpdates: new Ui.Setting(Ui.ComponentType.Toggle, 'Notify about updates', "The plugin will notify you if there's an update available :D", false),
            KBLiveSend: new Ui.Setting(Ui.ComponentType.Toggle, 'Live send mode', "Messages will be sent to the chatbox as you type.", false),
            KBTypingIndicator: new Ui.Setting(Ui.ComponentType.Toggle, 'Send typing indicator', "Some people prefer to not let people know when they are typing. If this is you here ya go!", true),
            TwitchSetup: new Ui.Setting(Ui.ComponentType.Button, 'Setup Twitch', "Setup simultaneous sending to Twitch and OSC", null, null, null),
            KBVersionCheck: new Ui.Setting(Ui.ComponentType.Button, 'Check for updates', "Check for updates rn!!!!!", null, null, null),
            KBOpenRepo: new Ui.Setting(Ui.ComponentType.Button, 'Plugin Repo', "View this plugin's repo on GitHub", null, null, null),
            KBVersion: new Ui.Setting(Ui.ComponentType.Text, 'KBVersion'),
        },
    },
    General: {
        XSOverlay: {
            AllowAdminPermissions: new Ui.Setting(Ui.ComponentType.Toggle, 'Allow Admin Permissions', "desc", false),
        },
        Accessibility: {
            DominantHand: new Ui.Setting(Ui.ComponentType.Dropdown, 'Dominant Hand', "", 'Right', ['Right', 'Left']),
            Language: new Ui.Setting(Ui.ComponentType.Dropdown, 'Language', "description", 'English', ['English']),
            Crowdin: new Ui.Setting(Ui.ComponentType.Button, 'Crowdin', "", 'box-arrow-up-right', null, null),
        },
        Mouse: {
            InputMethod: new Ui.Setting(Ui.ComponentType.Dropdown, 'Input Method', "", 'MouseEmulation', ['TouchInput', 'MouseEmulation']),
            AutomaticMouseControl: new Ui.Setting(Ui.ComponentType.Toggle, 'Automatic Mouse Control', "", false, null, null, new Ui.ParentSetting('InputMethod', 1)),
            DoubleClickDelay: new Ui.Setting(Ui.ComponentType.Slider, 'Double Click Delay', "", 0.25, [0, 1, 0.01], 'ms', new Ui.ParentSetting('InputMethod', 1)),
            PointerScale: new Ui.Setting(Ui.ComponentType.Slider, 'Pointer Scale', "", 1, [0.25, 1, 0.05], '%')
        },
        User_Interface: {
            HideTooltips: new Ui.Setting(Ui.ComponentType.Toggle, 'Hide Tooltips', "", false),
        },
        Miscellaneous: {
            DiscordRichPresence: new Ui.Setting(Ui.ComponentType.Toggle, 'Discord Rich Presence', "", true),
            LowBatterySound: new Ui.Setting(Ui.ComponentType.Toggle, 'Low Battery Sound', "", true),
            LowBatteryWarningPercent: new Ui.Setting(Ui.ComponentType.Slider, 'Low Battery Warning Percent', "", 15, [5, 25, 1], '%'),
            HapticsStrength: new Ui.Setting(Ui.ComponentType.Slider, 'Haptic Feedback', "", 1, [0, 1, 0.01], '%')
        },
        Analytics: {
            SendAnalytics: new Ui.Setting(Ui.ComponentType.Toggle, 'Send Analytics',
                "Data sent is completely anonymous and not tied to you as an individual, and is not considered as \"personally identifiable.\" ; Data collected goes towards making the software better. <br><br>Data includes:<br> - System Specs<br> - HMD Model<br> - Changed Settings<br> - Error / Crash Reporting<br> - VR Application<br> - Startup / Shutdown",
                true),
        },
        _: {
            VersionNumber: new Ui.Setting(Ui.ComponentType.Text, 'VersionNumber'),
        },
    },
    Overlays: {
        Capture: {
            CaptureMethod: new Ui.Setting(Ui.ComponentType.Dropdown, 'Window Capture API', "", 'Windows Graphics Capture', ['WindowsGraphicsCapture', 'BitBlt']),
            // ShowWindowPreviews: new Ui.Setting(Ui.ComponentType.Toggle, 'Show Window Thumbnails', true) NOTE:: DEPRECATED
        },
        Overlay_Behavior: {
            AutoRecenter: new Ui.Setting(Ui.ComponentType.Toggle, 'Auto Recenter', "", true),
            InvertScaleGesture: new Ui.Setting(Ui.ComponentType.Toggle, 'Invert Scale Gesture', "", false),
        },
        Overlay_Display: {
            CurvedOverlays: new Ui.Setting(Ui.ComponentType.Toggle, 'Curved Overlays', "", true),
            OverlayCurveBias: new Ui.Setting(Ui.ComponentType.Slider, 'Overlay Curve Bias', "", 1.5, [0.1, 5, 0.01], null, new Ui.ParentSetting('CurvedOverlays', true)),
            OverlayClipAngle: new Ui.Setting(Ui.ComponentType.Slider, 'Overlay Clip Angle', "", 68, [0, 180, 1], '°'),
            OverlayDefaultScale: new Ui.Setting(Ui.ComponentType.Slider, 'Default Scale', "", 1, [0, 1, 0.01], '%'),
            OverlayDefaultOpacity: new Ui.Setting(Ui.ComponentType.Slider, 'Default Opacity', "", 1, [0, 1, 0.01], '%'),
            OverlayDefaultMinFPS: new Ui.Setting(Ui.ComponentType.Slider, 'Default Min FPS', "", 30, [1, 144, 1], 'FPS'),
            OverlayDefaultMaxFPS: new Ui.Setting(Ui.ComponentType.Slider, 'Default Max FPS', "", 60, [1, 144, 1], 'FPS'),
        },
        Overlay_Movement_and_Handling: {
            AimTarget: new Ui.Setting(Ui.ComponentType.Dropdown, 'Target', "", 'Mixed', ['Mixed', 'HMD', 'Controller']),
            AimAtTarget: new Ui.Setting(Ui.ComponentType.Toggle, 'Aim At Target', "", true),
            GrabSensitivity: new Ui.Setting(Ui.ComponentType.Slider, 'Grab Sensitivity', "", 0.3, [0, 1, 0.01], '%'),
            PushSpeed: new Ui.Setting(Ui.ComponentType.Slider, 'Push Pull Speed', "", 0.03, [0, 0.2, 0.01]),
            ScaleSpeed: new Ui.Setting(Ui.ComponentType.Slider, 'Scale Speed', "", 3, [1, 6, 0.01]),
            PositionDampening: new Ui.Setting(Ui.ComponentType.Slider, 'Position Dampening', "", 15, [1, 100, 1]),
            RotationDampening: new Ui.Setting(Ui.ComponentType.Slider, 'Rotation Dampening', "", 15, [1, 100, 1]),
            SmartRollAngle: new Ui.Setting(Ui.ComponentType.Slider, 'Smart Roll Angle', "", 50, [10, 180, 1], '°')
        },
    },
    Wrist: {
        Wrist_Overlay: {
            AllowWristMovement: new Ui.Setting(Ui.ComponentType.Toggle, 'Allow Movement', "", true),
            HideWristOverlay: new Ui.Setting(Ui.ComponentType.Toggle, 'Hide Wrist Overlay', "", false),
            WristOpacity: new Ui.Setting(Ui.ComponentType.Slider, 'Opacity', "", 1, [0.1, 1, 0.01], '%'),
            WristClipAngle: new Ui.Setting(Ui.ComponentType.Slider, 'Clip Angle', "", 60, [10, 180, 1], '°'),
            ResetWristPosition: new Ui.Setting(Ui.ComponentType.Button, 'Reset Wrist Position', "", '', '', ''),
        },
        Date_and_Time:
        {
            ContinentalTimeFormat: new Ui.Setting(Ui.ComponentType.Toggle, '24 Hour Clock', "", false),
            DateFormat: new Ui.Setting(Ui.ComponentType.Dropdown, 'Date Format', "", 'DD / MM / YY', ['DD / MM / YY', 'MM / DD / YY', 'YY / MM / DD'])
        },
        Battery_Widget: {
            AlwaysShowDetailedInformation: new Ui.Setting(Ui.ComponentType.Toggle, 'Always Show Details', "", false),
            DefaultShowBatteryPercentage: new Ui.Setting(Ui.ComponentType.Toggle, 'Show Percentage by Default', "", true),
            BatteryFontScale: new Ui.Setting(Ui.ComponentType.Slider, 'Battery Font Size', "", 16, [12, 24, 1], 'px')
        },
        Media_Player: {
            AutoMediaDetection: new Ui.Setting(Ui.ComponentType.Toggle, 'Smart Media Controls', "", false),
        }
    },
    Theme: {
        Base_Theme: {
            UseDarkTheme: new Ui.Setting(Ui.ComponentType.Toggle, 'Dark Mode', "", true),
            AdaptiveColor: new Ui.Setting(Ui.ComponentType.Slider, 'Adaptive Color Strength', "", 0.1, [0, 1, 0.01], '%'),
            MediaThemeing: new Ui.Setting(Ui.ComponentType.Toggle, 'Media Themeing', "", false),
        },
        Accent_Color: {
            AccentColorR: new Ui.Setting(Ui.ComponentType.Slider, 'Red Amount', "", 0.2, [0, 1, 0.01], '%'),
            AccentColorG: new Ui.Setting(Ui.ComponentType.Slider, 'Green Amount', "", 0.82, [0, 1, 0.01], '%'),
            AccentColorB: new Ui.Setting(Ui.ComponentType.Slider, 'Blue Amount', "", 0.4, [0, 1, 0.01], '%'),
        },
    },
    Notifications: {
        Appearance: {
            NotificationScale: new Ui.Setting(Ui.ComponentType.Slider, 'Scale', "", 0.25, [0.1, 2, 0.001]),
            NotificationPositionX: new Ui.Setting(Ui.ComponentType.Slider, 'Position X', "", 0, [-2, 2, 0.001]),
            NotificationPositionY: new Ui.Setting(Ui.ComponentType.Slider, 'Position Y', "", -0.125, [-2, 2, 0.001]),
            NotificationPositionZ: new Ui.Setting(Ui.ComponentType.Slider, 'Position Z', "", 0.55, [-2, 2, 0.001]),
            SmallNotificationTest: new Ui.Setting(Ui.ComponentType.Button, 'Test Small', "", '', '', ''),
            LargeNotificationTest: new Ui.Setting(Ui.ComponentType.Button, 'Test Large', "", '', '', ''),
        }
    },
    Experimental: {
        Experimental_Settings: {
            ForceHigherQualityOverlays: new Ui.Setting(Ui.ComponentType.Toggle, 'Force High Quality Overlays', "Forces SteamVR to scale the compositor layer to 125% render scale. <br>This requires a restart of SteamVR.", false),
            BlockInputToBackgroundApplication: new Ui.Setting(Ui.ComponentType.Toggle, 'Block Inputs To Background App', "Blocks all input to background applications when focusing any overlay.", false),
            InputBlockingBehavior: new Ui.Setting(Ui.ComponentType.Dropdown, 'Blocking Behavior', "", 'LayoutMode', ['Hover', 'LayoutMode'], null, new Ui.ParentSetting('BlockInputToBackgroundApplication', true)),
        }
    },
    Night_Light: {
        _: {
            NightLight: new Ui.Setting(Ui.ComponentType.Toggle, 'Enable Night Light', "", false),
            NightLightIntensity: new Ui.Setting(Ui.ComponentType.Slider, 'Intensity', "", 0.5, [0, 1, 0.01], '%')
        },
        Schedule: {
            NightLightSchedule: new Ui.Setting(Ui.ComponentType.Toggle, 'Use Schedule', "", false),
            NightLightStartHour: new Ui.Setting(Ui.ComponentType.Slider, 'Start Time', "", 0.5, [1, 12, 1], null, new Ui.ParentSetting('NightLightSchedule', true)),
            NightLightStartAMPM: new Ui.Setting(Ui.ComponentType.Toggle, 'AM - PM', "", null, null, null, new Ui.ParentSetting('NightLightSchedule', true)),
            NightLightEndHour: new Ui.Setting(Ui.ComponentType.Slider, 'End Time', "", 0.5, [1, 12, 1], null, new Ui.ParentSetting('NightLightSchedule', true)),
            NightLightEndAMPM: new Ui.Setting(Ui.ComponentType.Toggle, 'AM - PM', "", null, null, null, new Ui.ParentSetting('NightLightSchedule', true)),
        }
    },
    Easter_Eggs: {
        Why_Are_They_Called_Easter_Eggs: {
            EasterEggBee: new Ui.Setting(Ui.ComponentType.Toggle, 'Beeeeeeeeeeeeeeeee!!!!', "", false, null, null, null, new Ui.ParentAchievement('SUMMONBEE', true))
        },
    },
    Support: {
        Support_Links: {
            Documentation: new Ui.Setting(Ui.ComponentType.Button, 'Docs', "", 'xsoverlay', 'styled', null),
            Discord: new Ui.Setting(Ui.ComponentType.Button, 'Discord', "", 'discord', 'styled', null),
            Twitter: new Ui.Setting(Ui.ComponentType.Button, 'Twitter', "", 'twitter', 'styled', null),
            Github: new Ui.Setting(Ui.ComponentType.Button, 'Github', "", 'github', 'styled', null),
            Steam: new Ui.Setting(Ui.ComponentType.Button, 'Steam', "", 'steam', 'styled', null),
        }
    },
    Bindings: {
        _: {
            Bindings: new Ui.Setting(Ui.ComponentType.Text, 'Bindings', "description"),
        }
    }
}

const Settings = {
    UiPages: []
};

function Initialize(name) {
    ui_canvas.pageRoot = document.getElementById('uiContainer');
    ui_canvas.uiRoot = Ui.CreateElement(ui_canvas.pageRoot, Ui.HtmlType.div, ['background', 'theme-dark']);
    ui_canvas.pageContainer = Ui.CreateElement(ui_canvas.uiRoot, Ui.HtmlType.div, ['page-wrapper', 'absolute']);
    InitializeUI();
    Api.Connect(name);
    SubscribeToApiEvents();
}

function InitializeUI() {
    PopulateSidebar();
    CreateUIPages();
    OnSwitchPage('General');
}

function PopulateSidebar() {
    ui_canvas.sidebar = Ui.CreateElement(ui_canvas.uiRoot, Ui.HtmlType.div, ['side-bar', 'theme-mid', 'absolute']);
    ui_canvas.sidebar_selector = Ui.CreateElement(ui_canvas.sidebar, Ui.HtmlType.div, ['side-bar-selector', 'absolute']);
    sidebar.s_list = Ui.CreateElement(ui_canvas.sidebar, Ui.HtmlType.div, ['side-bar-button-container']);

    var i = 0;
    Object.keys(sidebarButtons).forEach(function (key) {
        sidebarButtons[key] = Ui.CreateElement(sidebar.s_list, Ui.HtmlType.button, ['side-bar-button']);

        sidebarButtons[key].addEventListener("click", function (e) {
            setTimeout(function () { sidebarButtons[key].blur(); }, 150); //deselect button
            OnSwitchPage(key);
            e.preventDefault;
        });


        var icon = Ui.CreateElement(sidebarButtons[key], Ui.HtmlType.img, ['side-bar-button-icon', 'theme-font-contrast']);
        icon.classList.add(`bi-${sidebarButtonIconNames[i]}`);

        var text = Ui.CreateElement(sidebarButtons[key], Ui.HtmlType.div, ['side-bar-button-text']);
        text.innerHTML = key.replaceAll('_', ' ');
        text.setAttribute('translationKey', key);

        i++;

        if (i < Object.keys(sidebarButtons).length)
            Ui.Divider(sidebar.s_list, 'sidebar-divider');
    });
}

function CreateUIPages() {
    for (var pageLayout in SettingsLayout) {
        var uiRoot = Ui.CreateElement(ui_canvas.pageContainer, Ui.HtmlType.div, ['page-container', 'theme-dark']);
        uiRoot.setAttribute('id', `Page_${pageLayout}`);

        var pageHeader = Ui.CreateElement(uiRoot, Ui.HtmlType.div, ['page-header']);
        var pageHeaderText = Ui.CreateElement(pageHeader, Ui.HtmlType.div, ['page-header-text']);
        pageHeaderText.innerHTML = `${pageLayout.replaceAll('_', ' ')}`
        pageHeaderText.setAttribute('translationKey', pageLayout);

        if (pageLayout == 'Theme') {
            Ui.CreateElement(uiRoot, Ui.HtmlType.div, ['background-splash']);
            uiRoot.classList.replace('page-container', 'page-container-no-overflow')
        }

        var sections = [];
        for (var sectionLayout in SettingsLayout[pageLayout]) {
            var settingsLayout = SettingsLayout[pageLayout][sectionLayout];
            var numOfSettings = Object.keys(settingsLayout).length;
            var sectionTitle = sectionLayout;

            var createdSection = new Ui.Section(sectionTitle, numOfSettings, uiRoot);
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
                    case Ui.ComponentType.Text:
                        Ui.Description(createdSection.Background, desc, `${settingDefinition.internalName}_Desc`);
                        break;

                    case Ui.ComponentType.Button:
                        Ui.Button(settingDefinition, createdSection.Background);
                        break;

                    case Ui.ComponentType.Toggle:
                        Ui.Toggle(settingDefinition, name, defaultState, opts, createdSection.Background);
                        break;

                    case Ui.ComponentType.Slider:
                        Ui.Slider(settingDefinition, name, defaultState, opts, opts1, createdSection.Background, 300);
                        break;

                    case Ui.ComponentType.Dropdown:
                        Ui.Dropdown(settingDefinition, name, defaultState, opts, createdSection.Background, 300);
                        break;
                }

                if (desc != "" && type != Ui.ComponentType.Text) {
                    Ui.Description(createdSection.Background, desc, `${settingDefinition.internalName}_Desc`);
                }

                index++;
                if (index < Object.keys(settingsLayout).length) {
                    Ui.Divider(createdSection.Background, 'divider', setting);
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
        Api.Send(Api.Commands.OpenSteamVRBindings, null, null);
    }
}

// Message Handling Functions
function SubscribeToApiEvents() {
    Api.Client.Socket.addEventListener('open', () => {
        Api.Send(Api.Commands.SubscribeToEvents, JSON.stringify(
            [
                "Theme",
                "Languages",
                "LanguageMap",
            ]
        ));
        Api.Send(Api.Commands.RequestGetSettings, null, null);
        Api.Send(Api.Commands.RequestAchievementInformation, null, null);
    });

    Api.Client.Socket.addEventListener('message', function message(data) {
        HandleMessages(data);
    });

    Api.Client.Socket.addEventListener('close', () => {
        console.log(`${Api.ApiObject.sender} websocket was disconnected. Attempting reconnect.`);
        setTimeout(function () {
            Api.Connect('settings');
            SubscribeToApiEvents();
            console.log("Reconnecting...");
        }, 1000);
    });
}


function HandleMessages(msg) {
    var decoded = Api.Parse(msg);
    switch (decoded.Command) {
        case 'UpdateSteamAvatar':
            // Wrist.ProfileImage.src = `data:image/png;base64,${decoded.RawData}`
            break;

        case 'UpdateTheme':
            Common.ApplyTheme(decoded.JsonData);
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
            Ui.CheckForSettingsThatRelyOnSteamAchievements(decoded.JsonData);
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
    Ui.DropdownRepopulate(SettingsLayout.General.Accessibility.Language);
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

            Ui.UpdateSliderUI(sliderR, value.r);
            Ui.UpdateSliderUI(sliderG, value.g);
            Ui.UpdateSliderUI(sliderB, value.b);
        }
        else if (key == 'NotificationOffsets') {
            var sliderX = document.getElementById('NotificationPositionX');
            var sliderY = document.getElementById('NotificationPositionY');
            var sliderZ = document.getElementById('NotificationPositionZ');

            Ui.UpdateSliderUI(sliderX, value.x);
            Ui.UpdateSliderUI(sliderY, value.y);
            Ui.UpdateSliderUI(sliderZ, value.z);
        }
        else {
            var uiElement = document.getElementById(key);
            if (uiElement != null) {
                switch (uiElement.getAttribute('uiType')) {
                    case 'toggle':
                        uiElement.checked = value;
                        break;

                    case 'slider':
                        Ui.UpdateSliderUI(uiElement, value);
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

                Ui.CheckForSettingsThatRelyOnChangedSetting(key, value);
            }
        }
    }

    var appVersionText = document.getElementById("VersionNumber_Desc");
    appVersionText.innerHTML = `Build ${data.VersionNumber}`;

    console.log("Settings Updated.");
}