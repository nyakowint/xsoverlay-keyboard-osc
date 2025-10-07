# XSOverlay Keyboard Chatbox

If you are migrating from [OVR Toolkit](https://store.steampowered.com/app/1068820/OVR_Toolkit/) to [XSOverlay](https://store.steampowered.com/app/1173510/XSOverlay/) and miss the chatbox-on-keyboard, or otherwise want chatbox functionality, this addon/plugin may be for you!
  
> [!NOTE]
> This is an unofficial addon to XSOverlay, as there is no first-party plugin support in XSOverlay (as of 2025-10-06). \
> This plugin uses [BepInEx](https://docs.bepinex.dev/index.html) to carry out patches, add functionality and whatnot. \

> [!CAUTION]
> Last tested build: Build 680. \
> Newer patches *should* work but use with caution - random things might break due to changes by Xiexe \
> *(do not report bugs to them without removing the plugin first, thank you)* \
> The keyboard will get a large rework soon, so this plugin may break at that time too. :P

## Preview

(images are slightly out of date, but it pretty much looks the same)

| Icon                                                                                                                          | Bar                                                                                                                                  |
|-------------------------------------------------------------------------------------------------------------------------------|--------------------------------------------------------------------------------------------------------------------------------------|
| ![Icon preview](https://github.com/nyakowint/xsoverlay-keyboard-osc/assets/24845294/d43accef-d457-4d00-8b1f-3754e1edaa74)     | ![osc bar preview](https://github.com/nyakowint/xsoverlay-keyboard-osc/assets/24845294/61d71541-1cda-4222-bdbf-8f96fa602e0b)         |
| ![Settings preview](https://github.com/nyakowint/xsoverlay-keyboard-osc/assets/24845294/53179e68-1f21-46ec-89a7-9f3d649bbc14) | ![Version checker preview](https://github.com/nyakowint/xsoverlay-keyboard-osc/assets/24845294/6aadbcc6-263c-443d-8ffb-fce062c2cbc9) |

## Download/Install

Open a **PowerShell** window, and run the following command:

```pwsh
Invoke-WebRequest -UseBasicParsing "https://raw.githubusercontent.com/nyakowint/xsoverlay-keyboard-osc/main/install.ps1" | Invoke-Expression
```

If you install steam games to an uncommon location you will need to enter the path to your XSOverlay folder. \
To find it, open Steam, go to XSOverlay > Manage > Browse local files)

To update or remove the plugin, run the above script again and select your desired option. \

If you're having trouble, try [manual installation](#manual-installation)

## Usage instructions

1. Enable OSC. For VRChat you can find this in the Action Menu (Options > OSC > Enabled)
2. Open the XSOverlay keyboard
3. Press the message icon on the right hand side of the keyboard, under the lock button
4. Congrats! Type away

Optionally, open XSOverlay's settings (Settings > KeyboardOSC) and change the plugin options to your liking \
(this may break with an update, if it does you can still access the settings through [localhost:42071 in your browser](http://localhost:42071/apps/_UI/Default/Settings.html) until a better way comes along)

Troubleshooting/other info is at the bottom of this readme.

# Shortcut keys/text macros

Use the following shortcut keys for quick access to certain actions:

| Shortcut Key                        | Function                                                                                                  |
|-------------------------------------|-----------------------------------------------------------------------------------------------------------|
| <kbd>ESC</kbd>                      | Clear current text                                                                                        |
| <kbd>END</kbd>                      | Clear last sent message (equivalent to pressing "Clear Chatbox" in radial menu)                           |
| <kbd>TAB</kbd>                      | Toggle silent message (indicated by orange text, disables your typing indicator and chatbox noise as well) |
| <kbd>INSERT</kbd>                   | Replace current text with your last message (does not send)                                               |
| <kbd>Backspace</kbd> / <kbd>Delete</kbd> | Delete last character from right or left respectively                                                     |
| <kbd>CTRL</kbd> + <kbd>C</kbd>      | Copy current text to clipboard                                                                            |
| <kbd>CTRL</kbd> + <kbd>V</kbd>      | Paste text from your clipboard                                                                            |
| <kbd>CTRL</kbd> + <kbd>Backspace</kbd> | Delete last word (kinda broken lol)                                                                       |
| <kbd>ENTER</kbd>                    | Send message to the chatbox (behavior depends on settings)                                                |

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

Note that the emojis do not look good in vrchat's chatbox at all lmao 

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
- something else, bug me about it shrug

If you need help or have concerns please create an discussion for help or issue for bugs.


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

### Uninstall plugin

Removing the plugin can be done in one of two ways:
- Run the powershell script above and select `Remove` (option 3)
- Follow the manual install steps in reverse order. Delete `BepInEx`, `doorstop_config.ini`, `winhttp.dll` and other non
  overlay files from your XSOverlay folder.

## Build from source

Check the .csproj or actions workflow

but if you wanna build this just drop the necessary dlls from `XSOverlay_Data/Managed` into `refs`, restore and build w/
Release config. Plugin dll will be in `builds` folder