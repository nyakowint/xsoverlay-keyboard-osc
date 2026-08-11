# XSOverlay Keyboard Chatbox

If you use [XSOverlay](https://store.steampowered.com/app/1173510/XSOverlay/) and want OSC chatbox functionality, or you're migrating from [OVR Toolkit](https://store.steampowered.com/app/1068820/OVR_Toolkit/)'s,
this plugin is for you!

The plugin uses [BepInEx](https://docs.bepinex.dev/index.html) to add our own chatbox above the keyboard!

> [!IMPORTANT]
> Last tested build: Build 680. \
> Being a mod/plugin, random things might break due to changes by the XSO developer Xiexe! \
> *(do not report bugs to them without removing your plugins first!)* \

> [!IMPORTANT]
> Newest Beta 688 is currently incompatible with the plugin!
> 
> Before this beta ships, Xiexe plan to add OSC Keyboard Input for VRChat via STT or regular typing, which will finally make this plugin obsolete. (thank you)
>
> The plugin will receive updates on the [dev branch](../dev) until those features are added.

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
3. Press the message icon on the right hand side of the keyboard, under the lock button
4. Congrats! Type away

To change settings: Open XSOverlay's settings and find the Keyboard Icon Change the plugin options to your liking

(Troubleshooting/extras at the bottom of README)

# Shortcut keys/text macros

Use the following shortcuts for quick access to certain actions:

| Keybind                                 | Function                                                                                                   |
|-----------------------------------------|------------------------------------------------------------------------------------------------------------|
| <kbd>ESC</kbd>                          | Clear current text                                                                                         |
| <kbd>END</kbd>                          | Clear last sent message (equivalent to pressing "Clear Chatbox" in radial menu)                            |
| <kbd>TAB</kbd>                          | Toggle silent message (indicated by orange text, disables your typing indicator and chatbox noise as well) |
| <kbd>INSERT</kbd>                       | Replace current text with your last message (does not auto send)                                           |
| <kbd>Backspace</kbd> / <kbd>Delete</kbd> | Delete last character from right or left respectively                                                      |
| <kbd>CTRL</kbd> + <kbd>C</kbd>          | Copy current text to clipboard                                                                             |
| <kbd>CTRL</kbd> + <kbd>V</kbd>          | Paste text from your clipboard                                                                             |
| <kbd>CTRL</kbd> + <kbd>Backspace</kbd>  | Delete last word (kinda broken lol)                                                                        |
| <kbd>ENTER</kbd>                        | Send message to the chatbox (behaviour depends on your settings)                                           |

Full compatibility with OSC in alternate platforms (resonite/chillout/others) is not guaranteed. \
If it adheres mostly to VRChat OSC addresses it should be fine.

There are also a few text macro shortcuts built in:

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

The bar used for typing may have positional quirks until it's moved for the first time. I consider this a
non-issue.

If you can't seem to get OSC to work, try one of these:

- Restart VRChat before trying anything else. OSC as a whole will just break sometimes.
- Change the OSC port used by XSOverlay, instructions how to do this -> > [XSOverlay Docs](https://xsoverlay.vercel.app/commonissues#ports-bindings) < (
  it does not use OSCQuery as of writing, so this is probably your issue)
- Reset your OSC config?

If this plugin's settings dont show up in the menu, or the pages are white/blank it's likely:

- XSOverlay has updated enough to break the plugin (most likely)
- You are using an outdated version of the plugin (check releases)
- You are using a custom theme and it is conflicting somehow. (this update isn't out tho)
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
