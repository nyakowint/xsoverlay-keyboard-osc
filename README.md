# XSOverlay Keyboard OSC

A personal plugin to make chatbox typing in VRChat easier for me. Inspired by the OSC button found inside OVR Toolkit.

## Installation

> [!NOTE]
> This is not officially supported. Use at your own risk.\
> This should work on any recent version of XSOverlay starting with build 627! (20th Oct.)


1. [Follow the BepInEx install guide](https://docs.bepinex.dev/articles/user_guide/installation/index.html) into
   XSOverlay.
2. Download both the plugin DLL **and** `BepInEx.cfg` from [Releases](../../releases/latest)
3. **:bangbang:** Move `KeyboardOSC.dll` into `<xso folder>/BepInEx/plugins` folder,
   and `BepInEx.cfg` into `<xso folder>/BepInEx/config` folder.
    - **Make sure you have done the second part.** if you dont then you will have a quite useless plugin :L
    - or set it yourself: `HideManagerGameObject = true`
4. Start XSOverlay
5. You should have a message icon on the right side keyboard toolbar, press it. \
   A bar should pop up above the keyboard. If not, move the keyboard around and it should pop up.
7. Make sure you have OSC enabled in VRChat or the Chatbox mod for chillout
8. congration you done it

### Removing the plugin

- Delete `BepInEx`, `doorstop_config.ini`, `winhttp.dll` and other non overlay files from your XSOverlay folder

## Features/Usage

Send messages to the chatbox just like in OVR Toolkit!

Use the following shortcut keys:
| Shortcut Key | Function   
| ---------------- | --------
| <kbd>TAB</kbd> | Toggle silent msg (orange text)
| <kbd>Backspace</kbd> or <kbd>Delete</kbd> | Delete last character from right (bksp) or left (del)
| <kbd>ESC</kbd> | Clear current text
| <kbd>END</kbd> | Clear last sent message (equivalent to pressing "Clear Chatbox" in radial menu)
| <kbd>INSERT</kbd> | Replace current text with your last message
| <kbd>CTRL</kbd> + <kbd>C</kbd> | Copy current text to clipboard
| <kbd>CTRL</kbd> + <kbd>V</kbd> | Paste text from your clipboard
| <kbd>CTRL</kbd> + <kbd>Backspace</kbd> | Delete last word (works very jankily) |
| <kbd>ENTER</kbd> | Send message to the chatbox!

I cannot guarantee full functionality with the CVR chatbox mod, as this is built with VRChat's OSC routes in mind. They were identical last I checked.

## Unplanned features

- Any type of "now playing" feature. It's out of scope for the plugin lol
- Text-to-Speech, STT & STTS. If someone wants to implement this PRs are welcome but it's not in my interest.

## Known Issues

- Positioning is still a little weird, especially if you scale the keyboard

- ~~Steam might not instantly detect XSOverlay as closed.~~
   - This was *not* a bie/plugin issue, Xiexe has since fixed this. woohoo

If you have any other annoyances raise an issue and I might be able to fix it

## Build from source

Check the .csproj or actions workflow

but if you wanna build this just drop the dlls from `XSOverlay_Data/Managed` into `refs`, restore and build w/ Release
config. dll will be in `builds` folder

(or moved to your plugins folder if you used debug and have XSO on your C drive loool)

Why a mod for a vr overlay?
I like XSOverlay better than OVRTK. No, you _dont_ have to agree with me.
lol
