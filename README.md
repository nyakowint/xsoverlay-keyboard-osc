# XSOverlay Keyboard OSC

A extension for XSOverlay to make text communication in VRChat easier.


> [!NOTE]
> This is a third-party modification to [XSOverlay](https://store.steampowered.com/app/1173510/XSOverlay/). Use at your own discretion\
> Last tested with build 649. It should work on any recent version.

## Auto Install

Navigate to your XSOverlay folder, open a PowerShell window, and paste the following \
(If you're unsure open Steam and go to XSOverlay > Manage > Browse local files)

```pwsh
Invoke-WebRequest -UseBasicParsing "https://raw.githubusercontent.com/nyakowint/xsoverlay-keyboard-osc/main/install.ps1" | Invoke-Expression
```

If you have any issues try manually installing with the instructions below.


## Manual Installation
1. [Follow the BepInEx install guide](https://docs.bepinex.dev/articles/user_guide/installation/index.html) into
   XSOverlay.
2. Download both the plugin DLL **and** `BepInEx.cfg` from [Releases](../../releases/latest)
3. **IMPORTANT**: Move `KeyboardOSC.dll` into `<xso folder>/BepInEx/plugins` folder,
   and `BepInEx.cfg` into `<xso folder>/BepInEx/config` folder.
    - **Make sure you have done the second part.** if you dont then you will have a quite useless plugin :L
    - or set it yourself: `HideManagerGameObject = true`
4. Start XSOverlay
5. If you did everything right you should have a message icon on the keyboard toolbar (right). \
Click it. A bar should pop up above the keyboard. If not, move the
   keyboard around and it should pop up.
1. congration you done it
   - You may also want to disable XSO analytics while using the plugin, but that's up to you, i'm not the one getting error logs XD

### Removing the plugin

- Delete `BepInEx`, `doorstop_config.ini`, `winhttp.dll` and other non overlay files from your XSOverlay folder

# Features/Usage

Send messages to the chatbox just like in OVR Toolkit!

Use the following shortcut keys:
| Shortcut Key | Function   
| ---------------- | --------
| <kbd>TAB</kbd> | Toggle silent msg (orange text, disables typing indicator and chatbox noise)
| <kbd>ESC</kbd> | Clear current text
| <kbd>Backspace</kbd> or <kbd>Delete</kbd> | Delete last character from right (bksp) or left (del)
| <kbd>END</kbd> | Clear last sent message (equivalent to pressing "Clear Chatbox" in radial menu)
| <kbd>F6</kbd> | Toggle "partial send" mode (Send messages as you're typing them)
| <kbd>INSERT</kbd> | Replace current text with your last message
| <kbd>CTRL</kbd> + <kbd>C</kbd> | Copy current text to clipboard
| <kbd>CTRL</kbd> + <kbd>V</kbd> | Paste text from your clipboard
| <kbd>CTRL</kbd> + <kbd>Backspace</kbd> | Delete last word (this one is weird as holding ctrl doesnt actually mean holding ctrl) |
| <kbd>ENTER</kbd> | Send message to the chatbox!

## Unplanned features

- Any type of "now playing" feature. It's out of scope for the plugin lol
- Text-to-Speech, STT & STTS. If someone wants to implement this PRs are welcome but it's not in my interest.

## Known Issues

- The bar will be positioned significantly higher than intended until moved/scaled. I consider this a non-issue
  
If you find any, create an issue so i can remember to try and fix it when i feel like it

If you still need help you can find me in my dev server [discord](https://discord.gg/BrUacrw4cy)

## Build from source

Check the .csproj or actions workflow

but if you wanna build this just drop the dlls from `XSOverlay_Data/Managed` into `refs`, restore and build w/ Release
config. dll will be in `builds` folder

## Why a mod for a vr overlay?

* I like the control scheme of XSOverlay better than OVRTK. No, you _dont_ have to agree with me.
* Why wait for it to be added when you can do it yourself? lol
