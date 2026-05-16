import * as Api from '../../../../_Shared/api.js';
import * as Ui from '../js/uiComponents.js';

const SettingsLayout = {
    Keyboard_OSC: {
        _: {
            KBVersion: new Ui.Setting(Ui.ComponentType.Text, 'KBVersion'),
            KBCheckForUpdates: new Ui.Setting(Ui.ComponentType.Toggle, 'Notify about updates', "The plugin will notify you if there's an update available :D", true),
            KBLiveSend: new Ui.Setting(Ui.ComponentType.Toggle, 'Live send mode', "If enabled messages will be sent to the chatbox as you type.", false),
            KBTypingIndicator: new Ui.Setting(Ui.ComponentType.Toggle, 'Send typing indicator', "Some prefer to not let others know when they are typing. If this is you then here ya go!", true),
            KBDisableMaxLength: new Ui.Setting(Ui.ComponentType.Toggle, 'Disable max length', "Disable the character limit on the keyboard bar. This is pointless to enable for VRChat as you will be cut off at 160chars", false),
            KBVersionCheck: new Ui.Setting(Ui.ComponentType.Button, 'Check for updates', "Check for updates rn!!!!!", null, null, null),
            KBOpenRepo: new Ui.Setting(Ui.ComponentType.Button, 'Plugin Repo', "View this plugin's repo on GitHub", null, null, null),
        }
    }
};

InjectKBOSCTab();

function InjectKBOSCTab() {
    const sidebarList = document.querySelector('.side-bar-button-container');
    const pageWrapper = document.querySelector('.page-wrapper');
    if (!sidebarList || !pageWrapper) {
        console.error('[KBOSC] Could not find settings page structure to inject into');
        return;
    }

    // Sidebar button (inserted at top)
    const kboscBtn = document.createElement('button');
    kboscBtn.className = 'side-bar-button';

    const icon = document.createElement('i');
    icon.className = 'side-bar-button-icon theme-font-contrast bi-keyboard-fill';
    kboscBtn.appendChild(icon);

    const label = document.createElement('div');
    label.className = 'side-bar-button-text';
    label.innerHTML = 'Keyboard OSC';
    kboscBtn.appendChild(label);

    const divider = document.createElement('div');
    divider.className = 'sidebar-divider';
    sidebarList.insertBefore(divider, sidebarList.firstChild);
    sidebarList.insertBefore(kboscBtn, sidebarList.firstChild);

    // KBO page (appended to page wrapper, hidden by default)
    const kboscPage = Ui.CreateElement(pageWrapper, Ui.HtmlType.div, ['page-container', 'theme-dark']);
    kboscPage.id = 'Page_Keyboard_OSC';
    kboscPage.style.display = 'none';

    const pageHeader = Ui.CreateElement(kboscPage, Ui.HtmlType.div, ['page-header']);
    const pageHeaderText = Ui.CreateElement(pageHeader, Ui.HtmlType.div, ['page-header-text']);
    pageHeaderText.innerHTML = 'Keyboard OSC';

    const sectionLayout = SettingsLayout.Keyboard_OSC._;
    const createdSection = new Ui.Section('_', Object.keys(sectionLayout).length, kboscPage);
    let index = 0;
    for (const setting in sectionLayout) {
        const def = sectionLayout[setting];
        def.internalName = setting;
        def.sectionID = '_';

        switch (def.type) {
            case Ui.ComponentType.Text:
                Ui.Description(createdSection.Background, def.description, `${setting}_Desc`);
                break;
            case Ui.ComponentType.Button:
                Ui.Button(def, createdSection.Background);
                break;
            case Ui.ComponentType.Toggle:
                Ui.Toggle(def, def.displayName, def.defaultState, def.opts, createdSection.Background);
                break;
            case Ui.ComponentType.Slider:
                Ui.Slider(def, def.displayName, def.defaultState, def.opts, def.opts1, createdSection.Background, 300);
                break;
            case Ui.ComponentType.Dropdown:
                Ui.Dropdown(def, def.displayName, def.defaultState, def.opts, createdSection.Background, 300);
                break;
        }

        if (def.description !== '' && def.type !== Ui.ComponentType.Text)
            Ui.Description(createdSection.Background, def.description, `${setting}_Desc`);

        index++;
        if (index < Object.keys(sectionLayout).length)
            Ui.Divider(createdSection.Background, 'divider', setting);
    }

    // Tab switching
    kboscBtn.addEventListener('click', function () {
        setTimeout(function () { kboscBtn.blur(); }, 150);
        document.querySelectorAll('.page-container, .page-container-no-overflow').forEach(function (p) {
            if (p !== kboscPage) p.style.animation = '0.3s ease fade-out forwards';
        });
        kboscPage.style.display = '';
        kboscPage.style.animation = '0.3s ease fade-in forwards';
        document.querySelectorAll('.side-bar-button').forEach(function (b) {
            b.classList.remove('side-bar-button-selected');
            if (b.firstElementChild) b.firstElementChild.classList.remove('selected-icon');
        });
        kboscBtn.classList.add('side-bar-button-selected');
        icon.classList.add('selected-icon');
    });

    // Hide our page when any other sidebar button is clicked
    sidebarList.addEventListener('click', function (e) {
        const btn = e.target.closest('.side-bar-button');
        if (btn && btn !== kboscBtn) {
            kboscPage.style.animation = '0.3s ease fade-out forwards';
            kboscBtn.classList.remove('side-bar-button-selected');
            icon.classList.remove('selected-icon');
        }
    });

    // Tap into the existing socket for KBO settings
    if (Api.Client && Api.Client.Socket) {
        Api.Client.Socket.addEventListener('message', function (data) {
            const decoded = Api.Parse(data);
            if (decoded.Command === 'UpdateSettings') SetKBOMenuStates(decoded.JsonData);
        });
        if (Api.Client.Socket.readyState === WebSocket.OPEN)
            Api.Send(Api.Commands.RequestGetSettings, null, null);
        else
            Api.Client.Socket.addEventListener('open', function () {
                Api.Send(Api.Commands.RequestGetSettings, null, null);
            }, { once: true });
    }
}

function SetKBOMenuStates(data) {
    for (const key in SettingsLayout.Keyboard_OSC._) {
        if (data[key] === undefined) continue;
        const el = document.getElementById(key);
        if (!el) continue;
        switch (el.getAttribute('uiType')) {
            case 'toggle': el.checked = data[key]; break;
            case 'slider': Ui.UpdateSliderUI(el, data[key]); break;
        }
    }
    if (data.KBVersion) {
        const kbv = document.getElementById('KBVersion_Desc');
        if (kbv) kbv.innerHTML = 'Version ' + data.KBVersion;
    }
}
