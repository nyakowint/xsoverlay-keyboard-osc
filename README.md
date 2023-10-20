# XSOverlay Keyboard OSC

A personal plugin to make chatbox typing in VRChat easier for me. Inspired by the OSC button found inside OVR Toolkit.
I'm mostly sharing it so others can use it if they want to

## Installation

> [!NOTE]
> Make sure you have switched to the `beta` branch of XSOverlay. You can do this under Properties > Betas \
> This *might* work on the live branch but I don't use it and thus haven't tested it ~ YMMV. \
> Latest tested build: Build 625 UI3.0 RC5


1. [Follow the BepInEx install guide](https://docs.bepinex.dev/articles/user_guide/installation/index.html) into
   XSOverlay.
2. Download both the plugin DLL **and** `BepInEx.cfg` from [Releases](../../releases/latest)
3. **:bangbang: IMPORTANT :bangbang:**: Move `KeyboardOSC.dll` into `<xso folder>/BepInEx/plugins` folder,
   and `BepInEx.cfg` into `<xso folder>/BepInEx/config` folder.
    - **Make sure you have done the second part.** if you dont then you will have a quite useless plugin :L
    - or set it yourself: `HideManagerGameObject = true`
4. Start XSOverlay
5. If you did everything right you should have a message icon on the keyboard toolbar (right). \
Click it. A bar should pop up above the keyboard. If not, move the
   keyboard around and it should pop up.
6. congration you done it

### Removing the plugin

- Delete `BepInEx`, `doorstop_config.ini`, `winhttp.dll` and other non overlay files from your XSOverlay folder

## Features/Usage

Send messages to the chatbox just like in OVR Toolkit!

Use the following shortcut keys:
| Shortcut Key | Function   
| ---------------- | --------
| <kbd>TAB</kbd> | Toggle silent msg (orange text)
| <kbd>ESC</kbd> | Clear current text
| <kbd>Backspace</kbd> or <kbd>Delete</kbd> | Delete last character from right (bksp) or left (del)
| <kbd>END</kbd> | Clear last sent message (equivalent to pressing "Clear Chatbox" in radial menu)
| <kbd>INSERT</kbd> | Replace current text with your last message
| <kbd>CTRL</kbd> + <kbd>C</kbd> | Copy current text to clipboard
| <kbd>CTRL</kbd> + <kbd>V</kbd> | Paste text from your clipboard
| <kbd>CTRL</kbd> + <kbd>Backspace</kbd> | Delete last word (this one is weird as holding ctrl doesnt actually mean
holding ctrl) |
| <kbd>ENTER</kbd> | Send message to the chatbox!

## Unplanned features

- Any type of "now playing" feature. It's out of scope for the plugin lol
- Text-to-Speech, STT & STTS. If someone wants to implement this PRs are welcome but it's not in my interest.

## Known Issues

- Positioning is still a little weird, especially if you scale the keyboard

Steam might not instantly detect XSOverlay as closed. Not a big deal but this is out of my control just so you know

If you find any other issues create an issue so i can remember to try and fix it when i feel like it

## Build from source

Check the .csproj or actions workflow

but if you wanna build this just drop the dlls from `XSOverlay_Data/Managed` into `refs`, restore and build w/ Release
config. dll will be in `builds` folder

(or moved to your plugins folder if you used debug and have XSO on your C drive loool)

## Why a mod for a vr overlay?

* I like XSOverlay better than OVRTK. No, you _dont_ have to agree with me.
* Xiexe plate
  of [feature requests](https://github.com/Xiexe/XSOverlay-Issue-Tracker/issues?q=is%3Aissue+is%3Aopen+label%3A"feature+req.")
  is far too long for me to wait for it to be added. lol.
