import * as Api from '../../../../_Shared/api.js';
import * as Ui from '../js/uiComponents.js';

const Commands = {
    Ready: 'KBOSCReady',
    State: 'KBOSCState',
    Send: 'KBOSCSend',
    Typing: 'KBOSCTyping',
    Clipboard: 'KBOSCClipboard',
    ClipboardData: 'KBOSCClipboardData',
    Config: 'KBOSCConfig',
};

const Placeholder = 'Captured chatbox text';
const StockPlaceholder = 'Captured voice text';
const LiveSendDelay = 1300;
const TypingThrottle = 750;
const ValuePollInterval = 200;

const Chat = {
    active: false,
    silent: false,
    firstLiveChunk: true,
    lastMsg: '',
    lastValue: '',
    typing: false,
    typingSentAt: 0,
    liveTimer: null,
    pollTimer: null,
    focusCheck: null,
    focusId: null,
    hasFocus: false,
    lastRefocus: 0,
    syncing: false,
    socket: null,
    config: {
        version: '',
        liveSend: false,
        typingIndicator: true,
        disableMaxLength: false,
        maxLength: 144,
        macros: [],
    },
    wrap: null,
    input: null,
    toggle: null,
    counter: null,
    macros: null,
};

Initialize();

function Initialize() {
    InjectStyles();
    WatchKeyboard();
    WatchSocket();
    document.addEventListener('keydown', OnKeyDown, true);
    window.addEventListener('pagehide', () => { if (Chat.active) SetChatMode(false); });
    Log('chat bar loaded');
}

function Log(text) {
    console.log(`[KBOSC] ${text}`);
    const socket = Api.Client.Socket;
    if (socket && socket.readyState === WebSocket.OPEN) Api.Send('Log', null, `[KBOSC] ${text}`);
}

function WatchKeyboard() {
    const observer = new MutationObserver(() => Attach());
    observer.observe(document.body, { childList: true, subtree: true });
    Attach();
}

function Attach() {
    const wrap = document.getElementById('keyboard-voice-wrap');
    const input = document.getElementById('keyboard-voice-input');
    if (!wrap || !input || wrap.dataset.kbosc === 'attached') return;
    wrap.dataset.kbosc = 'attached';

    Chat.macros?.destroy();
    Chat.macros = null;

    Chat.wrap = wrap;
    Chat.input = input;
    Chat.toggle = CreateToggle();
    Chat.counter = CreateCounter();

    wrap.insertBefore(Chat.toggle, wrap.firstChild);
    BuildMacroMenu();
    const actions = document.getElementById('keyboard-voice-actions');
    if (actions) wrap.insertBefore(Chat.counter, actions);
    else wrap.appendChild(Chat.counter);

    input.addEventListener('input', () => OnValueChanged());
    wrap.addEventListener('pointerdown', OnWrapPointerDown, true);

    const container = document.getElementById('keyboard-container');
    container?.addEventListener('pointerdown', OnKeyPointerDown, true);
    container?.addEventListener('touchstart', OnKeyPointerDown, { capture: true, passive: false });

    if (Chat.active && Chat.lastValue) input.value = Chat.lastValue;
    Chat.lastValue = input.value;
    ApplyChatUi();
    if (Chat.active) RetakeFocus();
    Log('attached to the keyboard bar');
}

function CreateToggle() {
    const button = document.createElement('button');
    button.type = 'button';
    button.id = 'kbosc-toggle';
    button.className = 'button keyboard-control';
    button.title = 'Chatbox mode';
    button.setAttribute('aria-label', 'Chatbox mode');
    button.setAttribute('aria-pressed', 'false');

    const icon = document.createElement('span');
    icon.className = 'keyboard-control-icon';
    icon.innerHTML = '<i class="bi bi-chat-dots-fill" aria-hidden="true" style="font-size:20px"></i>';
    icon.setAttribute('aria-hidden', 'true');
    button.appendChild(icon);

    button.addEventListener('pointerdown', event => {
        event.preventDefault();
        SetChatMode(!Chat.active);
    });
    return button;
}

function BuildMacroMenu() {
    const macros = Chat.config.macros;
    if (!Chat.wrap || !macros.length) {
        Log(`macro menu skipped (bar: ${!!Chat.wrap}, macros: ${macros.length})`);
        return;
    }

    Chat.macros?.destroy();
    const controlSize = Number.parseFloat(
        getComputedStyle(Chat.wrap).getPropertyValue('--keyboard-control-size')) || 44;

    let menu;
    try {
        menu = Ui.Dropdown2(
            Chat.wrap,
            'down',
            macros.map(MacroLabel),
            MacroMenuTitle,
            34,
            5,
            160,
            controlSize,
        );
    } catch (error) {
        Log(`macro menu failed to build: ${error}`);
        return;
    }
    menu.container.id = 'kbosc-macros';
    menu.button.textContent = MacroMenuTitle;
    menu.button.title = 'Text macros';
    menu.button.setAttribute('aria-label', 'Text macros');
    menu.onChange(label => {
        const macro = macros.find(entry => MacroLabel(entry) === label);
        menu.button.textContent = MacroMenuTitle;
        if (!macro) return;
        InsertText(macro.glyph);
        if (Chat.hasFocus) Chat.input?.focus();
    });

    Chat.wrap.appendChild(menu.container);
    Chat.macros = menu;
    SyncKeyboardBar();
    Log(`macro menu built with ${macros.length} entries`);
}

const MacroMenuTitle = 'macros';

function MacroLabel(macro) {
    return `${macro.glyph}  ${macro.code.replace(/^\/\//, '')}`;
}

function CreateCounter() {
    const counter = document.createElement('span');
    counter.id = 'kbosc-counter';
    counter.setAttribute('aria-hidden', 'true');
    counter.textContent = `0/${Chat.config.maxLength}`;
    return counter;
}

function InjectStyles() {
    if (document.getElementById('kbosc-style')) return;
    const style = document.createElement('style');
    style.id = 'kbosc-style';
    style.textContent = `
#keyboard-voice-wrap.kbosc-chat:not(.voice-error) {
    box-shadow: 0 0 0 2px var(--theme-accent), 0 0 4px rgba(0, 0, 0, 0.5);
}
#keyboard-voice-wrap.kbosc-chat #keyboard-voice-input {
    display: block !important;
    pointer-events: auto !important;
    min-width: 320px !important;
}
#keyboard-voice-wrap.kbosc-chat #keyboard-voice-input,
#keyboard-voice-wrap.kbosc-chat #keyboard-voice-input:focus {
    box-shadow: none !important;
}
#keyboard-voice-wrap.kbosc-chat #keyboard-voice-actions {
    display: flex !important;
}
#keyboard-voice-wrap.kbosc-silent #keyboard-voice-input {
    color: var(--theme-warning);
}
#kbosc-macros {
    display: none;
    flex: 0 0 160px;
    width: 160px;
    height: var(--keyboard-control-size);
}
#keyboard-voice-wrap.kbosc-chat #kbosc-macros {
    display: block;
}
#kbosc-macros .dropdown,
#kbosc-macros .dropdown-button {
    width: 160px;
    height: var(--keyboard-control-size);
}
#kbosc-macros .dropdown-button {
    border-radius: var(--keyboard-control-radius);
}
#kbosc-macros .dropdown-list {
    top: calc(100% + 8px);
    bottom: auto;
    border-radius: var(--keyboard-panel-radius);
}
#kbosc-macros .dropdown-item {
    border-radius: calc(var(--keyboard-panel-radius) - 8px);
}
#kbosc-counter {
    display: none;
    align-items: center;
    flex: 0 0 auto;
    height: var(--keyboard-control-size);
    padding: 0 4px;
    box-sizing: border-box;
    color: var(--theme-contrasting);
    opacity: 0.7;
    font: 16px 'Roboto Flex', 'Noto Sans', sans-serif;
    white-space: nowrap;
}
#keyboard-voice-wrap.kbosc-chat #kbosc-counter {
    display: flex;
}
#keyboard-voice-wrap.kbosc-warn #kbosc-counter {
    color: var(--theme-warning);
    opacity: 1;
}
#keyboard-voice-wrap.kbosc-over #kbosc-counter {
    color: var(--theme-error);
    opacity: 1;
}`;
    document.head.appendChild(style);
}

function SetChatMode(on) {
    if (!Chat.input) return;
    Chat.active = on;

    if (on) {
        Chat.hasFocus = false;
        try {
            Chat.focusId = Api.RequestKeyboardInputFocus(Chat.input);
        } catch (error) {
            Log(`could not take keyboard focus: ${error}`);
            Chat.active = false;
            return;
        }

        setTimeout(() => {
            if (!Chat.active || Chat.hasFocus) return;
            Log('timed out getting keyboard focus, exiting chat mode');
            SetChatMode(false);
        }, 1000);
    } else {
        StopLiveTimer();
        SetTyping(false);
        Chat.silent = false;
        Chat.firstLiveChunk = true;
        if (Chat.focusId != null) Api.ReleaseKeyboardInputFocus();
        Chat.focusId = null;
        Chat.hasFocus = false;
    }

    SendCommand(Commands.State, { active: Chat.active });
    Log(`chat mode is now ${Chat.active ? 'on' : 'off'}`);
    ApplyChatUi();
}

function ApplyChatUi() {
    const { wrap, input, toggle } = Chat;
    if (!wrap || !input) return;

    wrap.classList.toggle('kbosc-chat', Chat.active);
    wrap.classList.toggle('kbosc-silent', Chat.active && Chat.silent);
    if (toggle) {
        toggle.classList.toggle('active', Chat.active);
        toggle.setAttribute('aria-pressed', Chat.active ? 'true' : 'false');
        toggle.title = Chat.active
            ? (Chat.silent ? 'Chatbox mode (silent)' : 'Chatbox mode')
            : 'Chatbox mode';
    }

    if (Chat.active) {
        ApplyPlaceholder();
        StartValuePolling();
    } else {
        StopValuePolling();
    }

    UpdateCounter();
    SyncKeyboardBar();
}

function SyncKeyboardBar() {
    if (!Chat.input || Chat.syncing) return;
    Chat.syncing = true;
    try {
        Chat.input.dispatchEvent(new Event('input', { bubbles: true }));
    } finally {
        Chat.syncing = false;
    }
    ApplyPlaceholder();
}

function ApplyPlaceholder() {
    if (!Chat.active || !Chat.input) return;
    const current = Chat.input.placeholder;
    if (current !== StockPlaceholder && current !== '' && current !== PlaceholderText()) return;
    Chat.input.placeholder = PlaceholderText();
}

function UpdateCounter() {
    const { wrap, counter, input } = Chat;
    if (!wrap || !counter || !input) return;

    const max = Chat.config.maxLength || 144;
    const length = input.value.length;
    counter.textContent = `${length}/${max}`;
    wrap.classList.toggle('kbosc-warn', length >= max * 0.6 && length < max * 0.85);
    wrap.classList.toggle('kbosc-over', length >= max * 0.85);
}

function ToggleSilent() {
    Chat.silent = !Chat.silent;
    if (Chat.silent) SetTyping(false);
    Log(`silent mode: ${Chat.silent}`);
    ApplyChatUi();
}

function OnValueChanged() {
    if (!Chat.input) return;
    const value = Chat.input.value;
    if (value === Chat.lastValue) return;

    if (!Chat.config.disableMaxLength && value.length > Chat.config.maxLength) {
        const caret = Chat.input.selectionStart;
        Chat.input.value = value.slice(0, Chat.config.maxLength);
        const clamped = Math.min(caret ?? Chat.config.maxLength, Chat.config.maxLength);
        Chat.input.setSelectionRange(clamped, clamped);
    }

    Chat.lastValue = Chat.input.value;
    UpdateCounter();
    if (!Chat.active) return;

    SetTyping(Chat.lastValue.length > 0);
    if (Chat.config.liveSend) StartLiveTimer();
}

function SetText(text, caretToEnd = true) {
    if (!Chat.input) return;
    Chat.input.value = text ?? '';
    if (caretToEnd) {
        const end = Chat.input.value.length;
        Chat.input.setSelectionRange(end, end);
    }
    OnValueChanged();
    SyncKeyboardBar();
}

function InsertText(text) {
    if (!Chat.input || !text) return;
    const value = Chat.input.value;
    const start = Chat.input.selectionStart ?? value.length;
    const end = Chat.input.selectionEnd ?? start;
    const combined = value.slice(0, start) + text + value.slice(end);
    Chat.input.value = combined;
    const caret = Math.min(start + text.length, combined.length);
    Chat.input.setSelectionRange(caret, caret);
    OnValueChanged();
    SyncKeyboardBar();
}

function ClearInput() {
    Chat.silent = false;
    Chat.firstLiveChunk = true;
    StopLiveTimer();
    SetText('');
    SetTyping(false);
    ApplyChatUi();
}

function SendChat() {
    const text = Chat.input?.value ?? '';
    if (!text.length) return;

    Chat.lastMsg = text;
    if (Chat.config.liveSend) {
        StopLiveTimer();
        PostChat(text, !Chat.silent && Chat.firstLiveChunk);
        Chat.firstLiveChunk = false;
    } else {
        PostChat(text, !Chat.silent);
    }

    SetTyping(false);
    ClearInput();
}

function SendLiveChunk() {
    if (!Chat.active || Chat.silent) return;
    const text = Chat.input?.value ?? '';

    PostChat(text, !Chat.silent && Chat.firstLiveChunk);
    Chat.firstLiveChunk = false;
    if (!text.length) SetTyping(false);
    else if (Chat.config.typingIndicator) ForceTyping(true);
}

function ClearChatbox() {
    PostChat('', false);
    Log('chatbox cleared');
}

function PostChat(text, sfx) {
    SendCommand(Commands.Send, { text, sfx: !!sfx });
}

function StartLiveTimer() {
    StopLiveTimer();
    Chat.liveTimer = setTimeout(SendLiveChunk, LiveSendDelay);
}

function StopLiveTimer() {
    if (Chat.liveTimer == null) return;
    clearTimeout(Chat.liveTimer);
    Chat.liveTimer = null;
}

function SetTyping(typing) {
    if (typing && (Chat.silent || !Chat.config.typingIndicator)) return;
    const now = Date.now();
    if (typing === Chat.typing && now - Chat.typingSentAt < TypingThrottle) return;
    ForceTyping(typing);
}

function ForceTyping(typing) {
    Chat.typing = typing;
    Chat.typingSentAt = Date.now();
    SendCommand(Commands.Typing, { typing });
}

function StartValuePolling() {
    if (Chat.pollTimer != null) return;
    Chat.pollTimer = setInterval(() => {
        if (!Chat.input) return;
        if (Chat.input.value !== Chat.lastValue) OnValueChanged();
        ApplyPlaceholder();
    }, ValuePollInterval);
}

function StopValuePolling() {
    if (Chat.pollTimer == null) return;
    clearInterval(Chat.pollTimer);
    Chat.pollTimer = null;
}

function PlaceholderText() {
    return Chat.silent ? `${Placeholder} (silent)` : Placeholder;
}

function OnKeyDown(event) {
    if (!Chat.active) return;

    const ctrl = event.ctrlKey || event.metaKey;
    switch (event.key) {
        case 'Enter':
            event.preventDefault();
            SendChat();
            return;
        case 'Escape':
            event.preventDefault();
            if (Chat.input?.value) ClearInput();
            else SetChatMode(false);
            return;
        case 'Tab':
            event.preventDefault();
            ToggleSilent();
            return;
        case 'Insert':
            event.preventDefault();
            SetText(Chat.lastMsg);
            return;
        case 'End':
            if (Chat.input?.value) return;
            event.preventDefault();
            ClearChatbox();
            return;
        case 'c':
        case 'C':
            if (!ctrl) return;
            event.preventDefault();
            CopyText();
            return;
        case 'v':
        case 'V':
            if (!ctrl) return;
            event.preventDefault();
            PasteText();
            return;
    }
}

function SelectedText() {
    const input = Chat.input;
    if (!input) return '';
    const start = input.selectionStart ?? 0;
    const end = input.selectionEnd ?? 0;
    return end > start ? input.value.slice(start, end) : input.value;
}

function OnKeyPointerDown(event) {
    if (!Chat.active || !Chat.input) return;
    if (event.type === 'pointerdown' && event.pointerType === 'touch') return;

    const target = event.target instanceof Element ? event.target : null;
    const key = target?.closest('.key');
    if (!key) return;

    if (key.querySelector('.bi-copy')) {
        event.preventDefault();
        event.stopPropagation();
        CopyText();
        return;
    }
    if (key.querySelector('.bi-clipboard')) {
        event.preventDefault();
        event.stopPropagation();
        PasteText();
        return;
    }

    if (!Chat.hasFocus || document.activeElement === Chat.input) return;
    Chat.lastRefocus = Date.now();
    Chat.input.focus();
}

function CopyText() {
    SendCommand(Commands.Clipboard, { action: 'copy', text: SelectedText() });
}

function PasteText() {
    SendCommand(Commands.Clipboard, { action: 'paste' });
}

function OnWrapPointerDown(event) {
    if (!Chat.active) return;
    const target = event.target instanceof Element ? event.target : null;
    if (!target) return;

    if (target.closest('#keyboard-voice-button')) return;

    const send = target.closest('#keyboard-voice-send');
    if (send) {
        event.preventDefault();
        event.stopPropagation();
        if (!send.disabled) SendChat();
        return;
    }

    const clear = target.closest('#keyboard-voice-clear');
    if (clear) {
        event.preventDefault();
        event.stopPropagation();
        ClearInput();
    }
}

function SendCommand(command, payload) {
    const socket = Api.Client.Socket;
    if (!socket || socket.readyState !== WebSocket.OPEN) return;
    Api.Send(command, payload == null ? null : JSON.stringify(payload), null);
}

function WatchSocket() {
    const announce = () => {
        SendCommand(Commands.Ready, null);
        SendCommand(Commands.State, { active: Chat.active });
    };

    const bind = () => {
        const socket = Api.Client.Socket;
        if (!socket || socket === Chat.socket) return;
        Chat.socket = socket;
        socket.addEventListener('message', OnSocketMessage);
        if (socket.readyState === WebSocket.OPEN) announce();
        else socket.addEventListener('open', announce, { once: true });
    };

    bind();
    setInterval(bind, 1000);
}

function OnSocketMessage(message) {
    let decoded;
    try {
        decoded = Api.Parse(message);
    } catch {
        return;
    }

    switch (decoded.Command) {
        case Commands.Config:
            ApplyConfig(decoded.JsonData);
            break;

        case Commands.ClipboardData:
            InsertText(decoded.JsonData?.text ?? '');
            break;

        case 'KeyboardInputFocusGranted':
            Chat.hasFocus = true;
            if (Chat.active) SendCommand(Commands.State, { active: true });
            break;

        case 'KeyboardInputFocusReleased':
            Chat.hasFocus = false;
            if (!Chat.active) return;
            if (decoded.JsonData?.reason === 'enter') {
                SendChat();
                RetakeFocus();
            } else if (decoded.JsonData?.reason === 'escape') {
                if (Chat.input?.value) ClearInput();
                else SetChatMode(false);
                if (Chat.active) RetakeFocus();
            } else {
                ScheduleFocusCheck();
            }
            break;
    }
}

function ApplyConfig(config) {
    if (!config) return;
    const macros = Array.isArray(config.macros)
        ? config.macros.filter(macro => macro?.code && macro?.glyph)
        : [];
    const macrosChanged = macros.length !== Chat.config.macros.length ||
        macros.some((macro, index) => macro.code !== Chat.config.macros[index]?.code);

    Chat.config = {
        version: config.version ?? Chat.config.version,
        liveSend: !!config.liveSend,
        typingIndicator: !!config.typingIndicator,
        disableMaxLength: !!config.disableMaxLength,
        maxLength: Number(config.maxLength) || 144,
        macros,
    };
    if (macrosChanged || !Chat.macros) BuildMacroMenu();
    Log(`plugin config debug: v${Chat.config.version}, live send: ${Chat.config.liveSend}, typing indicator: ` +
        `${Chat.config.typingIndicator}, msg max length: ${Chat.config.disableMaxLength ? 'off' : Chat.config.maxLength}, ` +
        `all macros: ${macros.length}`);
    if (!Chat.config.liveSend) StopLiveTimer();
    UpdateCounter();
}

function RetakeFocus() {
    if (!Chat.input) return;
    try {
        Chat.focusId = Api.RequestKeyboardInputFocus(Chat.input);
    } catch (error) {
        Log(`could not retake keyboard focus: ${error}`);
        SetChatMode(false);
    }
}

function ScheduleFocusCheck() {
    if (Chat.focusCheck != null) clearTimeout(Chat.focusCheck);
    Chat.focusCheck = setTimeout(() => {
        Chat.focusCheck = null;
        if (!Chat.active || Chat.hasFocus) return;
        Log('keyboard focus lost, leaving chat mode');
        SetChatMode(false);
    }, 250);
}
