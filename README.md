# XSOverlay Keyboard OSC
A personal plugin to make chatbox typing in VRChat easier for me. Inspired by the OSC button found inside OVR Toolkit.

### Why a mod for a vr overlay?
* personally I like XSOverlay better than OVRTK
* cause i can kekw

# This requires you set `HideManagerGameObject` to true in Bepinex config for xso builds 625 and later. probably a unity moment dont go blaming xiexe

i will fix soontm

I'm mostly sharing it so others can use it if they want to


## Installation

> [!NOTE]
> Make sure you have switched to the `beta` branch of XSOverlay. You can do this under Properties > Betas \
> This *might* work on the live branch but I don't use it and thus haven't tested it ~ YMMV. 

1. [Follow the BepInEx install guide](https://docs.bepinex.dev/articles/user_guide/installation/index.html) into XSOverlay.
2. Drop the plugin from [Releases](../../releases/latest) into `<xso folder>/BepInEx/plugins` folder.
3. Start XSOverlay
4. Use the message icon on the keyboard toolbar (right). A bar should pop up above the keyboard. If not, move the keyboard around and it should pop up.
   
## Features/Usage
Send messages to the chatbox just like in OVR Toolkit!

Use the following shortcut keys:
 | Shortcut Key  | Function   
 | ---------------- | --------
 | <kbd>TAB</kbd> | Toggle silent msg (orange text) 
 | <kbd>ESC</kbd> | Clear current text 
 | <kbd>Backspace</kbd> or <kbd>Delete</kbd> | Delete last character from right (bksp) or left (del)
 | <kbd>END</kbd> | Clear last sent message (equivalent to pressing "Clear Chatbox" in radial menu) 
 | <kbd>INSERT</kbd> | Replace current text with your last message 
 | <kbd>CTRL</kbd> + <kbd>C</kbd> | Copy current text to clipboard 
 | <kbd>CTRL</kbd> + <kbd>V</kbd> | Paste text from your clipboard 
 | <kbd>CTRL</kbd> + <kbd>Backspace</kbd> | Delete last word (this one is weird as holding ctrl doesnt actually mean holding ctrl) |
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

but if you wanna build this just drop the dlls from `XSOverlay_Data/Managed` into `refs`, restore and build w/ Release config. dll will be in `builds` folder 

(or moved to your plugins folder if you used debug and have XSO on your C drive loool)
