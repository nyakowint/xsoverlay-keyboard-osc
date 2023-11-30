# XSOverlay Keyboard OSC

A extension for XSOverlay to make text communication in VRChat easier.


> [!NOTE]
> This is a third-party modification to [XSOverlay](https://store.steampowered.com/app/1173510/XSOverlay/). Use at your
> own discretion\
> Last tested with build 649. It should work on any recent version.

## Auto Install

Open a PowerShell window, navigate to your XSOverlay folder and paste the following \
(If you're unsure open Steam and go to XSOverlay > Manage > Browse local files)

```pwsh
Invoke-WebRequest -UseBasicParsing "https://raw.githubusercontent.com/nyakowint/xsoverlay-keyboard-osc/main/install.ps1" | Invoke-Expression
```

Open XSOverlay, and hit that shiny new button on the right side of the keyboard!

If you're having trouble, try manually installing with the instructions below.

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
6. congration you done it
    - You may also want to disable XSO analytics while using the plugin, but that's up to you, i'm not the one getting
      error logs XD

### Removing the plugin

- Delete `BepInEx`, `doorstop_config.ini`, `winhttp.dll` and other non overlay files from your XSOverlay folder

# Features/Usage

Send messages to the chatbox just like in OVR Toolkit!

Use the following shortcut keys:
| Shortcut Key | Function   
| ---------------- | --------
| <kbd>ESC</kbd> | Clear current text
| <kbd>END</kbd> | Clear last sent message (equivalent to pressing "Clear Chatbox" in radial menu)
| <kbd>TAB</kbd> | Toggle silent msg (orange text, disables typing indicator and chatbox noise)
| <kbd>INSERT</kbd> | Replace current text with your last message
| <kbd>F6</kbd> | Send partial messages as you're typing (experimental)
| &nbsp; |
| <kbd>Backspace</kbd> or <kbd>Delete</kbd> | Delete last character from right or left respectively
| <kbd>CTRL</kbd> + <kbd>C</kbd> | Copy current text to clipboard
| <kbd>CTRL</kbd> + <kbd>V</kbd> | Paste text from your clipboard
| <kbd>CTRL</kbd> + <kbd>Backspace</kbd> | Delete last word (experimental)
| <kbd>ENTER</kbd> | Send message to the chatbox!

I cannot guarantee full functionality with the CVR chatbox mod, as this is built with VRChat's OSC routes in mind. They
were identical last I checked.

## Known Issues

- The bar will be positioned significantly higher than intended until you move it for the first time. I consider this a
  non-issue

If you find any, create an issue so i can remember to try and fix it when i feel like it

If you still need help you can find me in my dev server [discord](https://discord.gg/BrUacrw4cy)

## Build from source

Check the .csproj or actions workflow

but if you wanna build this just drop the dlls from `XSOverlay_Data/Managed` into `refs`, restore and build w/ Release
config. dll will be in `builds` folder

### Motivation

- vrchat's keyboard is so gosh darn buggy. I wasn't even able to type a full sentence before it malfunctioned. this bug is probably buried on their swamped canny page lol
- i like that ovr toolkit had this feature already, but i dont want to switch back to it
- u nerd lol
