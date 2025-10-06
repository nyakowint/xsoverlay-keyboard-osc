# XSOverlay Keyboard OSC

If you are migrating from [OVR Toolkit](https://store.steampowered.com/app/1068820/OVR_Toolkit/) to [XSOverlay](https://store.steampowered.com/app/1173510/XSOverlay/) and miss the chatbox-on-keyboard, or otherwise want chatbox functionality, this addon/plugin may be for you!
  
> [!NOTE]
> There is no first-party plugin support in XSOverlay (as of 2025-10-06). This plugin is applied using [BepInEx](https://docs.bepinex.dev/index.html). \

> [!CAUTION]
> Last tested build: Build 667. \
> Patch 680 and newer do not work atm. See https://github.com/nyakowint/xsoverlay-keyboard-osc/issues/5 \
> Newer patches *should* work, but use with caution - random things might break due to changes by Xiexe \
> *(do not report bugs to them without removing the plugin first, thank you)*

## Preview

(images are slightly out of date, but it pretty much looks the same)

|                                                                                                                               |                                                                                                                                      |
|-------------------------------------------------------------------------------------------------------------------------------|--------------------------------------------------------------------------------------------------------------------------------------|
| ![Icon preview](https://github.com/nyakowint/xsoverlay-keyboard-osc/assets/24845294/d43accef-d457-4d00-8b1f-3754e1edaa74)     | ![osc bar preview](https://github.com/nyakowint/xsoverlay-keyboard-osc/assets/24845294/61d71541-1cda-4222-bdbf-8f96fa602e0b)         |
| ![settings preview](https://github.com/nyakowint/xsoverlay-keyboard-osc/assets/24845294/53179e68-1f21-46ec-89a7-9f3d649bbc14) | ![version checker preview](https://github.com/nyakowint/xsoverlay-keyboard-osc/assets/24845294/6aadbcc6-263c-443d-8ffb-fce062c2cbc9) |

## Download/Install

Open a **PowerShell** window, change directory to your XSOverlay folder and paste the following script \
(To find it, open Steam, go to XSOverlay > Manage > Browse local files)

```pwsh
Invoke-WebRequest -UseBasicParsing "https://raw.githubusercontent.com/nyakowint/xsoverlay-keyboard-osc/main/install.ps1" | Invoke-Expression
```

If you're having trouble, try [manual installation](#manual-installation)

## Usage instructions

1. Enable OSC. For VRChat you can find this in the Action Menu (Options > OSC > Enabled)
2. Open the XSOverlay keyboard
3. Press the message icon on the right hand side of the keyboard, under the lock button
4. Congrats! Type away

Optionally, open XSOverlay's settings (Settings > KeyboardOSC) and change the plugin options to your liking \
(this may break with an update, you can go back with the tab under bindings)

> [!NOTE]
> The modified settings UI deliberately uses default theme.
> If you're a custom theme user (unreleased) and this bothers you enough feel free to pull request.


# Shortcut Keys

Use the following shortcut keys for quick access to certain actions:
| Shortcut Key | Function   
| ---------------- | --------
| <kbd>ESC</kbd> | Clear current text
| <kbd>END</kbd> | Clear last sent message (equivalent to pressing "Clear Chatbox" in radial menu)
| <kbd>TAB</kbd> | Toggle silent message (indicated by orange text, disables your typing indicator and chatbox noise as well)
| <kbd>INSERT</kbd> | Replace current text with your last message (does not send)
| &nbsp; |
| <kbd>Backspace</kbd> or <kbd>Delete</kbd> | Delete last character from right or left respectively
| <kbd>CTRL</kbd> + <kbd>C</kbd> | Copy current text to clipboard
| <kbd>CTRL</kbd> + <kbd>V</kbd> | Paste text from your clipboard
| <kbd>CTRL</kbd> + <kbd>Backspace</kbd> | Delete last word (kinda broken lol)
| <kbd>ENTER</kbd> | Send message to the chatbox! Has differing behavior depending on your settings.

I cannot guarantee full compatibility with OSC in alternate platforms (mostly resonite/chillout?) as this is made with VRC's routes in mind.  

## Twitch Message Setup

This will set you up for sending your messages sent through this plugin to your Twitch chat.

- Go to [Twitch Developer Console](https://dev.twitch.tv/console) and create an Application
- Set the redirect URI to `http://localhost:<WEB PORT>/apps/KeyboardOSC/twitchAuth.html`
  - Web port is usually WebSocketPort + 1 (if you didnt change it, it's currently `42071`) 
- Select Confidential for Client Type
- Go to Settings > KeyboardOSC
- Press the Twitch setup button
- In the webpage fill in your Client ID and Client Secret from Twitch Dev then press Authorize on both pages
- If everything works out (hopefully) you should get a toast message saying it's successful
  - Use silent message (Tab) to not send to twitch per message, if needed

## Troubleshooting

The bar used for typing may have positional quirks until you move it for the first time. I consider this a
non-issue. It tries its best :p

If you can't seem to get OSC to work, try one of these:

- Restart VRChat before trying anything else. OSC as a whole will just break sometimes.
- Change the OSC port used by XSOverlay, instructions how to do this -> > [XSOverlay Docs](https://xsoverlay.vercel.app/commonissues#ports-bindings) < (
  it does not use OSCQuery as of writing, so this is probably your issue)
- Reset your OSC config?

If this plugin's settings dont show up in the menu, it's likely:

- You are using a custom theme and it is conflicting somehow. As of writing this update is not fully out and i have no
  way of testing it lmao
- Either your XSOverlay or Plugin are outdated
- something else, bug me about it shrug

If you need help or have concerns please create an discussion for help or issue for bugs.


## Manual install

1. [Follow the BepInEx install guide](https://docs.bepinex.dev/articles/user_guide/installation/index.html) into
   XSOverlay.
2. Download both the plugin DLL **and** `BepInEx.cfg` from [Releases](../../releases/latest)
3. **IMPORTANT**: Move `KeyboardOSC.dll` into `<xso folder>/BepInEx/plugins` folder,
   and `BepInEx.cfg` into `<xso folder>/BepInEx/config` folder.
    - **Make sure you have done the second part.** if you dont then you will have a quite useless plugin :L
    - or set it yourself: `HideManagerGameObject = true`
4. Start XSOverlay

### Uninstall plugin

- Follow the install steps in reverse order. Delete `BepInEx`, `doorstop_config.ini`, `winhttp.dll` and other non
  overlay files from your XSOverlay folder.

## Build from source

Check the .csproj or actions workflow

but if you wanna build this just drop the necessary dlls from `XSOverlay_Data/Managed` into `refs`, restore and build w/
Release config. dll will be in `builds` folder
