using System.Collections.Generic;
using System.Linq;
using System.Timers;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using TMPro;
using UnityEngine;
using WindowsInput.Native;
using XSOverlay;

// ReSharper disable SwitchStatementMissingSomeEnumCasesNoDefault

namespace KeyboardOSC;

public static class ChatMode
{
    private static bool _isSilentMsg;
    private static bool _isFirstMsg;
    private static string _currentText = "";
    private static string _lastMsg = "";
    private static readonly ManualLogSource Logger = Plugin.PluginLogger;
    private static TextMeshProUGUI _oscBarText;
    private static TextMeshProUGUI _charCounter;
    private static List<KeyboardKey> _currentlyDownStickyKeys = [];
    private static Timer _eventsTimer = new(1300);

    public static void HandleKey(KeyboardKey.VirtualKeyEventData eventData)
    {
        var scanCode = eventData.Sender.UsingRawVirtualKeyCode
            ? (uint)eventData.KeyCode[0]
            : eventData.Sender.ScanCode[0];
        var shiftedField = AccessTools.Field(typeof(KeyboardKey), "IsShifted");
        var altedField = AccessTools.Field(typeof(KeyboardKey), "IsAlted");
        var isShifted = (bool)shiftedField.GetValue(eventData.Sender);
        var isAlted = (bool)altedField.GetValue(eventData.Sender); // altGr

        foreach (var key in eventData.KeyCode)
        {
            var character = Tools.ConvertVirtualKeyToUnicode(key, scanCode, isShifted, isAlted);
            ProcessKey(key, eventData, character);
        }
    }

    private static void ProcessKey(VirtualKeyCode key, KeyboardKey.VirtualKeyEventData data, string character)
    {
        var isCtrlHeld = _currentlyDownStickyKeys
            .Any(k => k.Key[0] is VirtualKeyCode.LCONTROL or VirtualKeyCode.RCONTROL);
        var sendTyping = PluginSettings.GetSetting<bool>("TypingIndicator").Value;
        var liveSendMode = PluginSettings.GetSetting<bool>("LiveSend").Value;
        switch (key)
        {
            // backspace/delete keys
            case VirtualKeyCode.BACK or VirtualKeyCode.DELETE:
            {
                if (_currentText.Length <= 0) return;
                if (isCtrlHeld)
                {
                    var lastSpaceIndex = _currentText.LastIndexOf(' ');
                    _currentText = lastSpaceIndex >= 0 ? _currentText.Substring(0, lastSpaceIndex) : "";
                    UpdateChatText(_currentText);
#if DEBUG
                    Logger.LogInfo("bulk deleting chat text: " + _currentText);
#endif
                    if (sendTyping) SendTyping(_currentText.Length != 0);
                    if (liveSendMode) _eventsTimer.Start();
                    return;
                }

                _currentText = _currentText.Remove(key is VirtualKeyCode.DELETE ? 0 : _currentText.Length - 1, 1);
                if (sendTyping) SendTyping(_currentText.Length != 0);
                if (liveSendMode) _eventsTimer.Start();
                UpdateChatText(_currentText);
                return;
            }
            // silent switch (no sound on send, no typing indicator)
            case VirtualKeyCode.TAB:
                _isSilentMsg = !_isSilentMsg;
                UpdateChatColor();
                Logger.LogInfo($"Silent mode: {_isSilentMsg}");
                return;
            // clear shortcut
            case VirtualKeyCode.ESCAPE:
                ClearInput();
                Logger.LogInfo("Input cleared");
                return;
            case VirtualKeyCode.END:
                Tools.SendOsc("/chatbox/input", string.Empty, true, false);
                Logger.LogInfo("Chatbox cleared");
                return;
            case VirtualKeyCode.INSERT:
                _currentText = _lastMsg;
                UpdateChatText(_currentText);
                _isFirstMsg = true;
                Logger.LogInfo("Inserted last input");
                return;
            // copy + paste
            case VirtualKeyCode.VK_C:
                if (!isCtrlHeld) break;
                GUIUtility.systemCopyBuffer = _currentText;
                return;
            case VirtualKeyCode.VK_V:
                if (!isCtrlHeld) break;
                _currentText += GUIUtility.systemCopyBuffer;
                UpdateChatText(_currentText);
                return;
            case VirtualKeyCode.RETURN:
                if (liveSendMode)
                {
                    Logger.LogInfo($"Sending message (live send enabled): {_currentText.ReplaceShortcodes()}");
                    _lastMsg = _currentText;
                    SendMessage(true);
                    ClearInput();
                }
                else
                {
                    Logger.LogInfo($"Sending message: {_currentText.ReplaceShortcodes()}");
                    SendMessage();
                }

                return;
        }

        // Normal character inputs
        if (sendTyping) SendTyping(_currentText.Length != 0);
        if (liveSendMode) _eventsTimer.Start();

        _currentText += character;
        UpdateChatText(_currentText);
    }

    private static void TimerElapsed(object sender, ElapsedEventArgs e)
    {
        if (_isSilentMsg) return;
        Logger.LogInfo("Timer elapsed, sending message");
        if (_currentText.IsNullOrWhiteSpace())
        {
            InputToChatbox(string.Empty, false);
            SendTyping(false);
        }

        var sendTyping = PluginSettings.GetSetting<bool>("TypingIndicator").Value;
        SendMessage(true);
        if (sendTyping) SendTyping(true);
    }

    private static void SendMessage(bool liveSend = false)
    {
        var triggerSfx = !_isSilentMsg && _isFirstMsg;

        if (liveSend)
        {
            _eventsTimer.Stop();
            if (_isFirstMsg) _isFirstMsg = false;
            InputToChatbox(_currentText.ReplaceShortcodes(), triggerSfx);
            SendTyping(false);
        }
        else
        {
            InputToChatbox(_currentText.ReplaceShortcodes(), !_isSilentMsg);
            SendTyping(false);

            _lastMsg = _currentText;
            ClearInput();
        }
    }

    /// <summary>
    /// Since i keep forgetting:
    /// /chatbox/input s b n Input text into the chatbox.
    /// If B is True, send the text in S immediately, bypassing the keyboard. If b is False, open the keyboard and populate it with the provided text.
    /// N is an additional bool parameter that when set to False will not trigger the notification SFX (defaults to True if not specified).
    /// </summary>
    private static void InputToChatbox(string text, bool triggerSfx = true)
    {
        Tools.SendOsc("/chatbox/input", text, true, triggerSfx);
    }

    private static void ClearInput()
    {
        UpdateChatText(string.Empty);
        _currentText = string.Empty;
        _isSilentMsg = false;
        _isFirstMsg = true;
        UpdateChatColor();
        Plugin.ReleaseStickyKeys.Invoke(Plugin.Instance.inputHandler, null);
    }

    private static void SendTyping(bool typing)
    {
        if (typing && _isSilentMsg) return;
        Tools.SendOsc("/chatbox/typing", typing);
    }

    public static void Setup(TextMeshProUGUI barText, TextMeshProUGUI charCounter)
    {
        _oscBarText = barText;
        _charCounter = charCounter;
        _eventsTimer.Elapsed += TimerElapsed;
        var stickyKeysField = AccessTools.Field(typeof(KeyboardInputHandler), "CurrentlyDownStickyKeys");
        _currentlyDownStickyKeys = (List<KeyboardKey>)stickyKeysField.GetValue(Plugin.Instance.inputHandler);
    }

    private static void UpdateChatColor()
    {
        _oscBarText.color =
            _isSilentMsg ? UIThemeHandler.Instance.T_WarningTone : UIThemeHandler.Instance.T_ConstrastingTone;
        _charCounter.color = _currentText.Length switch
        {
            >= 120 => UIThemeHandler.Instance.T_ErrTone,
            >= 85 => UIThemeHandler.Instance.T_WarningTone,
            _ => UIThemeHandler.Instance.T_ConstrastingTone
        };
    }

    private static void UpdateChatText(string text)
    {
        var disableMaxLength = PluginSettings.GetSetting<bool>("DisableMaxLength").Value;
        if (text.Length > 144 && !disableMaxLength)
        {
            text = text.Substring(0, 144);
        }

        XSTools.SetTMPUIText(_charCounter, $"{text.Length}/144");
        UpdateChatColor();

        XSTools.SetTMPUIText(_oscBarText, text);
    }

    private static string ReplaceShortcodes(this string input)
    {
        var shortcodes = new Dictionary<string, string>
        {
            { "//shrug", "¯\\_(ツ)_/¯" },
            { "//happy", "(¬‿¬)" },
            { "//tflip", "┬─┬" },
            { "//music", "🎵" },
            { "//cookie", "🍪" },
            { "//star", "⭐" },
            { "//hrt", "💗" },
            { "//hrt2", "💕" },
            { "//skull", "💀" },
            { "//skull2", "☠" },
            { "//rx3", "rawr x3" }
        };

        foreach (var shortcode in shortcodes)
        {
            input = input.Replace(shortcode.Key, shortcode.Value);
        }

        return input;
    }
}