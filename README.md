# XSOverlay Keyboard Chatbox

If you use [XSOverlay](https://store.steampowered.com/app/1173510/XSOverlay/) and want OSC chatbox functionality, or you're migrating from [OVR Toolkit](https://store.steampowered.com/app/1068820/OVR_Toolkit/)'s,
this plugin is for you!

The plugin uses [BepInEx](https://docs.bepinex.dev/index.html) to turn the keyboard's text bar into an OSC chatbox!

> [!IMPORTANT]
> Last tested build: Beta 688. \
> Being a mod/plugin, random things might break due to changes by the XSO developer Xiexe! \
> *(do not report bugs to them without removing your plugins first!)* \

> [!IMPORTANT]
> **Version 2.0.0 is only for beta 688 and newer**, where the keyboard became a web page.
> On older builds use [1.3.1](../../releases/tag/1.3.1) instead.
>
> Xiexe plan to add OSC keyboard input for VRChat (via STT or regular typing) themselves, which
> will finally make this plugin obsolete. (thank you)

## Preview

|                                                                                                                                     |                                                                                                                                  |
|-------------------------------------------------------------------------------------------------------------------------------------|----------------------------------------------------------------------------------------------------------------------------------|
| ![Icon preview](https://github.com/user-attachments/assets/2423b366-f3bb-499d-ac34-12dfc125d6b6)                                    | ![osc bar preview](https://github.com/user-attachments/assets/2800adc4-e107-42eb-b7d3-ca2948cd8c8e)                              |
| ![Chatbox result preview](https://github.com/user-attachments/assets/ac96da74-53be-4198-906c-a99f89419e5e) | ![Chatbox bar preview](https://github.com/user-attachments/assets/8a6ab58b-9a0b-43e7-96c8-14d2150a64f1) |

## Install

Quick install: Open a **PowerShell** window, and run the following command:

```pwsh
Invoke-WebRequest -UseBasicParsing "https://raw.githubusercontent.com/nyakowint/xsoverlay-keyboard-osc/main/install.ps1" | Invoke-Expression
```
You'll need to paste the path to your XSOverlay folder if you install steam games to an uncommon location.  \
To find it, open Steam and navigate to [XSOverlay] > Manage (cog) > Browse local files

To update or remove the chatbox plugin, run the above script again and select update/remove.

Having trouble? Do a [manual installation](#manual-installation)

## Usage instructions

1. Enable OSC. For VRChat you can find this in the Action Menu (Options > OSC > Enabled)
2. Open the XSOverlay keyboard
3. Press the chat bubble button on the left of the keyboard's text bar (next to the microphone)
4. Congrats! Type away — <kbd>ENTER</kbd> sends, and the button (or <kbd>ESC</kbd> on an empty
   bar) gives the keyboard back to Windows

While chat mode is on, the keyboard types into the chat bar instead of whatever app is focused,
so nothing leaks into your game.

The chat button decides where the bar sends, for anything that writes into it:

| Chat mode | Typing                     | XSOverlay's dictation (microphone)          |
|-----------|----------------------------|---------------------------------------------|
| off       | typed into Windows (stock) | stock — send types the transcript to Windows |
| on        | goes to the chat bar       | transcript goes to the chat bar → sent to the chatbox over OSC |

So dictating into the VRChat chatbox is just: chat mode on, microphone, then <kbd>ENTER</kbd>.
XSOverlay's own voice flow is untouched with chat mode off.

To change settings: Open XSOverlay's settings, go to the **Keyboard** page and scroll to the
**Chatbox** section.

(Troubleshooting/extras at the bottom of README)

# Shortcut keys/text macros

Use the following shortcuts for quick access to certain actions:

| Keybind                                 | Function                                                                                                   |
|-----------------------------------------|------------------------------------------------------------------------------------------------------------|
| <kbd>ESC</kbd>                          | Clear current text, or leave chat mode if the bar is already empty                                         |
| <kbd>END</kbd>                          | Clear the chatbox — only on an empty bar, otherwise it moves the caret as usual                            |
| <kbd>TAB</kbd>                          | Toggle silent message (indicated by orange text, disables your typing indicator and chatbox noise as well) |
| <kbd>INSERT</kbd>                       | Replace current text with your last message (does not auto send)                                           |
| <kbd>Backspace</kbd> / <kbd>Delete</kbd> | Delete a character before/after the caret                                                                  |
| <kbd>CTRL</kbd> + <kbd>C</kbd>          | Copy selected text (or the whole line) to clipboard                                                        |
| <kbd>CTRL</kbd> + <kbd>V</kbd>          | Paste text from your clipboard                                                                             |
| <kbd>ENTER</kbd>                        | Send message to the chatbox (behaviour depends on your settings)                                           |

Arrow keys, home/end and text selection all work now, since the bar is a real text field.

Full compatibility with OSC in alternate platforms (resonite/chillout/others) is not guaranteed. \
If it adheres mostly to VRChat OSC addresses it should be fine.

The keyboard's dedicated copy and paste keys work on the chat bar too (they do nothing at all in
the base build — XSOverlay ships them without a keycode).

There are also a few text macro shortcuts built in. Type them, or pick them from the **macros**
menu on the right end of the chat bar:

| Trigger     | Output           |
|-------------|------------------|
| `//shrug`   | `¯\\_(ツ)_/¯`    |
| `//happy`   | `(¬‿¬)`          |
| `//tflip`   | `┬─┬`            |
| `//music`   | `🎵`             |
| `//cookie`  | `🍪`             |
| `//star`    | `⭐`             |
| `//hrt`     | `💗`             |
| `//hrt2`    | `💕`             |
| `//skull`   | `💀`             |
| `//skull2`  | `☠`              |
| `//rx3`     | `rawr x3`        |

Note that the emojis do not look good in vrchat's chatbox font at all lmao

## Troubleshooting

The plugin logs to XSOverlay's own log as well as BepInEx's, so
`%userprofile%/AppData/LocalLow/Xiexe/XSOverlay/output_logs` is worth a look if the bar
misbehaves — search it for `[KBOSC]`.

If you can't seem to get OSC to work, try one of these:

- Restart VRChat before trying anything else. OSC as a whole will just break sometimes.
- Change the OSC port used by XSOverlay, instructions how to do this -> > [XSOverlay Docs](https://xsoverlay.vercel.app/commonissues#ports-bindings) < (
  it does not use OSCQuery as of writing, so this is probably your issue)
- Reset your OSC config?

If the chat button or the settings section don't show up, it's likely:

- XSOverlay has updated enough to break the plugin (most likely)
- You are using an outdated version of the plugin (check releases), or 2.0.0 on a build older
  than beta 688
- You are using a custom theme and it is conflicting somehow
- something else, pls create a github issue

For other concerns or help please create an issue or discussion post.


## Manual install

If the powershell script isn't working for you or you have other trouble, use these steps to install the plugin:

1. [Follow the BepInEx install guide](https://docs.bepinex.dev/articles/user_guide/installation/index.html) into
   XSOverlay.
2. Download both the plugin DLL **and** `BepInEx.cfg` from [Releases](../../releases/latest)
3. **IMPORTANT**: Move `KeyboardOSC.dll` into `<xso folder>/BepInEx/plugins` folder,
   and `BepInEx.cfg` into `<xso folder>/BepInEx/config` folder.
    - **Make sure you have done the second part.** if you dont then you will have a quite useless plugin :L
    - or set it yourself: `HideManagerGameObject = true`
4. Start XSOverlay

### Uninstall / Remove

Removing the plugin can be done in one of two ways:
- Run the powershell script above and select `Remove` (option 3)
- Follow the manual install steps in reverse order. Delete `BepInEx`, `doorstop_config.ini`, `winhttp.dll` and other non
  overlay files from your XSOverlay folder.

## Contributions/build from source

Check the .csproj or actions workflow

but if you wanna build this just drop the necessary dlls from `XSOverlay_Data/Managed` into `refs`, restore and build w/
Release config. Plugin dll will be in `builds` folder

You also need to add BepInEx's nuget source unless u wanna grab from ur bepinex folder
