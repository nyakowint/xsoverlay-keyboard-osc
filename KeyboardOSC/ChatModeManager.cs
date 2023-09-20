using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Logging;
using HarmonyLib;
using TMPro;
using UnityEngine;
using WindowsInput.Native;
using XSOverlay;

// ReSharper disable SwitchStatementMissingSomeEnumCasesNoDefault

namespace KeyboardOSC;

public static class ChatModeManager
{
    private static bool _isSilentMsg;
    private static string _currentText = "";
    private static string _lastMsg = "";
    private static DateTime _lastPingTime;
    private static readonly ManualLogSource Logger = Plugin.PluginLogger;
    private static TextMeshProUGUI _oscBarText;
    private static List<KeyboardKey> _currentlyDownStickyKeys = new();

    public static void HandleKey(KeyboardKey.VirtualKeyEventData eventData)
    {
        var scanCode = eventData.Sender.UsingRawVirtualKeyCode
            ? (uint) eventData.KeyCode[0]
            : eventData.Sender.ScanCode[0];
        var shiftedField = AccessTools.Field(typeof(KeyboardKey), "IsShifted");
        var isShifted = (bool) shiftedField.GetValue(eventData.Sender);

        foreach (var key in eventData.KeyCode)
        {
            var character = Tools.ConvertVirtualKeyToUnicode(key, scanCode, isShifted);
            ProcessKey(key, eventData, character);
        }
    }

    private static void ProcessKey(VirtualKeyCode key, KeyboardKey.VirtualKeyEventData data, string character)
    {
        var isCtrlHeld = _currentlyDownStickyKeys
            .Any(k => k.Key[0] is VirtualKeyCode.LCONTROL or VirtualKeyCode.RCONTROL);
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
                    Logger.LogInfo("bulk deleting chat text: " + _currentText);
                    return;
                }

                _currentText = _currentText.Remove(key is VirtualKeyCode.DELETE ? 0 : _currentText.Length - 1);
                UpdateChatText(_currentText);
                Logger.LogInfo("deleting chat text: " + _currentText);
                return;
            }
            // silent switch (no pop sound)
            case VirtualKeyCode.TAB:
                _isSilentMsg = !_isSilentMsg;
                UpdateChatColor();
                return;
            // clear shortcut
            case VirtualKeyCode.ESCAPE:
                _currentText = "";
                UpdateChatText(_currentText);
                Logger.LogInfo("clearing chat text");
                SendTyping(false);
                return;
            case VirtualKeyCode.END:
                SendMessage("/chatbox/input", string.Empty, true, false);
                return;
            case VirtualKeyCode.INSERT:
                _currentText = _lastMsg;
                UpdateChatText(_currentText);
                return;
            // copy + paste
            case VirtualKeyCode.VK_C:
                if (!isCtrlHeld) return;
                GUIUtility.systemCopyBuffer = _currentText;
                return;
            case VirtualKeyCode.VK_V:
                if (!isCtrlHeld) return;
                _currentText += GUIUtility.systemCopyBuffer;
                UpdateChatText(_currentText);
                return;
        }


        // send to vrchat
        if (key is VirtualKeyCode.RETURN)
        {
            if (_currentText.Length <= 0) return;
            Logger.LogInfo("sending chat text: " + _currentText);
            _lastMsg = _currentText;
            SendMessage("/chatbox/input", _currentText, true, _isSilentMsg);
            UpdateChatText("");
            _currentText = "";
            _isSilentMsg = false;
            Plugin.ReleaseStickyKeys.Invoke(Plugin.Instance.inputHandler, null);
            SendTyping(false);
            UpdateChatColor();
            return;
        }


        if (string.IsNullOrEmpty(character)) return;
        _currentText += character;
        UpdateChatText(_currentText);
        SendTyping(true);
        Logger.LogInfo("updating chat text with " + _currentText);
    }

    public static void SendTyping(bool typing)
    {
        if (DateTime.Now - _lastPingTime < TimeSpan.FromSeconds(2) && typing) return;
        _lastPingTime = DateTime.Now;
        if (typing)
        {
            Tools.SendOsc("/chatbox/typing", "true");
            return;
        }

        Tools.SendOsc("/chatbox/typing", "false");
    }

    private static void SendMessage(string address, string msg, bool now, bool sound)
    {
        Tools.SendOsc(address, msg, now, sound);
    }

    public static void Setup(TextMeshProUGUI obText)
    {
        _oscBarText = obText;
        var stickyKeysField = AccessTools.Field(typeof(KeyboardInputHandler), "CurrentlyDownStickyKeys");
        _currentlyDownStickyKeys = (List<KeyboardKey>) stickyKeysField.GetValue(Plugin.Instance.inputHandler);
    }

    private static void UpdateChatColor()
    {
        _oscBarText.color =
            _isSilentMsg ? UIThemeHandler.Instance.T_WarningTone : UIThemeHandler.Instance.T_ConstrastingTone;
    }

    private static void UpdateChatText(string text)
    {
        if (text.Length > 250)
        {
            text = text.Substring(0, 250);
        }

        XSTools.SetTMPUIText(_oscBarText, text);
    }
}