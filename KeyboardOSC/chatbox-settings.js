import * as Api from '../../../../_Shared/api.js';
import * as Ui from '../js/uiComponents.js';

const Commands = {
    Ready: 'KBOSCReady',
    Config: 'KBOSCConfig',
};

const SectionName = 'Chatbox';

const SectionLayout = {
    KBLiveSend: new Ui.Setting(Ui.ComponentType.Toggle, 'Live send mode',
        'If enabled messages will be sent to the chatbox as you type.', false),
    KBTypingIndicator: new Ui.Setting(Ui.ComponentType.Toggle, 'Send typing indicator',
        'Some prefer to not let others know when they are typing. If this is you then here ya go!', true),
    KBDisableMaxLength: new Ui.Setting(Ui.ComponentType.Toggle, 'Disable max length',
        'Disable the character limit on the chatbox bar. This is pointless to enable for VRChat as you will be cut off at 144 chars.', false),
    KBCheckForUpdates: new Ui.Setting(Ui.ComponentType.Toggle, 'Notify about updates',
        "The plugin will notify you if there's an update available :D", true),
    KBVersionCheck: new Ui.Setting(Ui.ComponentType.Button, 'Check for updates',
        'Check for updates rn!!!!!', 'arrow-repeat', null, null),
    KBOpenRepo: new Ui.Setting(Ui.ComponentType.Button, 'Plugin Repo',
        "View this plugin's repo on GitHub", 'github', null, null),
    KBVersion: new Ui.Setting(Ui.ComponentType.Text, 'KBVersion', 'Keyboard OSC'),
};

let socket = null;

WaitForKeyboardPage();
WatchSocket();

function Log(text) {
    console.log(`[KBOSC] ${text}`);
    const current = Api.Client.Socket;
    if (current && current.readyState === WebSocket.OPEN) Api.Send('Log', null, `[KBOSC] ${text}`);
}

function WaitForKeyboardPage(attempt = 0) {
    const page = document.getElementById('Page_Keyboard');
    if (!page) {
        if (attempt > 100) {
            Log('no keyboard settings page to add our section to');
            return;
        }
        setTimeout(() => WaitForKeyboardPage(attempt + 1), 100);
        return;
    }

    if (document.getElementById(SectionName)) return;
    BuildSection(page);
}

function BuildSection(page) {
    const settings = Object.keys(SectionLayout);
    const section = new Ui.Section(SectionName, settings.length, page);

    settings.forEach((name, index) => {
        const setting = SectionLayout[name];
        setting.internalName = name;
        setting.sectionID = SectionName;

        switch (setting.type) {
            case Ui.ComponentType.Text:
                Ui.Description(section.Background, setting.description, `${name}_Desc`);
                break;
            case Ui.ComponentType.Button:
                Ui.Button(setting, section.Background);
                break;
            case Ui.ComponentType.Toggle:
                Ui.Toggle(setting, setting.displayName, setting.defaultState, setting.opts, section.Background);
                break;
        }

        if (setting.description !== '' && setting.type !== Ui.ComponentType.Text) {
            Ui.Description(section.Background, setting.description, `${name}_Desc`);
        }

        if (index < settings.length - 1) Ui.Divider(section.Background, 'divider', name);
    });

    Log('chatbox settings added to the keyboard page');
}

function ApplyConfig(config) {
    if (!config) return;

    const values = {
        KBLiveSend: config.liveSend,
        KBTypingIndicator: config.typingIndicator,
        KBDisableMaxLength: config.disableMaxLength,
        KBCheckForUpdates: config.checkForUpdates,
    };

    for (const key in values) {
        const element = document.getElementById(key);
        if (element && element.getAttribute('uiType') === 'toggle') element.checked = !!values[key];
    }

    const version = document.getElementById('KBVersion_Desc');
    if (version) version.innerHTML = config.versionText || `Version ${config.version ?? '?'}`;
}

function WatchSocket() {
    const bind = () => {
        const current = Api.Client.Socket;
        if (!current || current === socket) return;
        socket = current;
        current.addEventListener('message', OnSocketMessage);
        if (current.readyState === WebSocket.OPEN) RequestConfig();
        else current.addEventListener('open', RequestConfig, { once: true });
    };

    bind();
    setInterval(bind, 1000);
}

function RequestConfig() {
    Api.Send(Commands.Ready, null, null);
}

function OnSocketMessage(message) {
    let decoded;
    try {
        decoded = Api.Parse(message);
    } catch {
        return;
    }

    if (decoded.Command === Commands.Config) ApplyConfig(decoded.JsonData);
}
