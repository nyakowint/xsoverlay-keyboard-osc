# XSOverlay Keyboard OSC

> [!NOTE]
> This is a third-party modification to [XSOverlay](https://store.steampowered.com/app/1173510/XSOverlay/). Use at your
> own discretion \
> Last tested with [beta] build 659. It should work on any recent version.

- Installation: [Automatic](#how-to-install) or [Manual](#manual-installation)
- [Usage instructions](#how-to-use)
- [Preview images](#preview)
- [Troubleshooting](#troubleshooting)

## How to install

Open a PowerShell window, navigate to your XSOverlay folder and paste the following \
(If you're unsure open Steam and go to XSOverlay > Manage > Browse local files)

```pwsh
Invoke-WebRequest -UseBasicParsing "https://raw.githubusercontent.com/nyakowint/xsoverlay-keyboard-osc/main/install.ps1" | Invoke-Expression
```

If you're having trouble, try [manually installing](#manual-installation) with the instructions below.

## How to use

1. Enable OSC. For VRChat you can find this in the Action Menu (Options > OSC > Enabled)
2. Open the XSOverlay keyboard
3. Press the message icon on the right hand side of the keyboard, under the lock button
4. congration you done it - Check out the [shortcut keys](#shortcut-keys)!

Optionally open XSOverlay Settings (Settings > KeyboardOSC) and change them to your liking \
(this may break with an update, you can go back with the tab under bindings)

> [!NOTE]
> The modified settings UI uses default theme on purpose to cause less annoyances for me.
> If you're a custom theme user (currently unreleased) and this bothers you enough feel free to pull request. i lazy

## Manual Installation

1. [Follow the BepInEx install guide](https://docs.bepinex.dev/articles/user_guide/installation/index.html) into
   XSOverlay.
2. Download both the plugin DLL **and** `BepInEx.cfg` from [Releases](../../releases/latest)
3. **IMPORTANT**: Move `KeyboardOSC.dll` into `<xso folder>/BepInEx/plugins` folder,
   and `BepInEx.cfg` into `<xso folder>/BepInEx/config` folder.
    - **Make sure you have done the second part.** if you dont then you will have a quite useless plugin :L
    - or set it yourself: `HideManagerGameObject = true`
4. Start XSOverlay

### Removing the plugin

- Follow the install steps in reverse order. Delete `BepInEx`, `doorstop_config.ini`, `winhttp.dll` and other non
  overlay files from your XSOverlay folder.

## Preview

|                                                                                                                               |                                                                                                                                      |
|-------------------------------------------------------------------------------------------------------------------------------|--------------------------------------------------------------------------------------------------------------------------------------|
| ![Icon preview](https://github.com/nyakowint/xsoverlay-keyboard-osc/assets/24845294/d43accef-d457-4d00-8b1f-3754e1edaa74)     | ![osc bar preview](https://github.com/nyakowint/xsoverlay-keyboard-osc/assets/24845294/61d71541-1cda-4222-bdbf-8f96fa602e0b)         |
| ![settings preview](https://github.com/nyakowint/xsoverlay-keyboard-osc/assets/24845294/53179e68-1f21-46ec-89a7-9f3d649bbc14) | ![version checker preview](https://github.com/nyakowint/xsoverlay-keyboard-osc/assets/24845294/6aadbcc6-263c-443d-8ffb-fce062c2cbc9) |

# Shortcut Keys

Send messages to the chatbox just like in OVR Toolkit!

Use the following shortcut keys:
| Shortcut Key | Function   
| ---------------- | --------
| <kbd>ESC</kbd> | Clear current text
| <kbd>END</kbd> | Clear last sent message (equivalent to pressing "Clear Chatbox" in radial menu)
| <kbd>TAB</kbd> | Toggle silent msg (orange text, disables typing indicator and chatbox noise)
| <kbd>INSERT</kbd> | Replace current text with your last message
| &nbsp; |
| <kbd>Backspace</kbd> or <kbd>Delete</kbd> | Delete last character from right or left respectively
| <kbd>CTRL</kbd> + <kbd>C</kbd> | Copy current text to clipboard
| <kbd>CTRL</kbd> + <kbd>V</kbd> | Paste text from your clipboard
| <kbd>CTRL</kbd> + <kbd>Backspace</kbd> | Delete last word (experimental)
| <kbd>ENTER</kbd> | Send message to the chatbox!

I cannot guarantee full functionality with the CVR chatbox mod, as this is built with VRChat's OSC routes in mind. They
were identical last I checked.

## Troubleshooting

The bar may be positioned significantly higher than intended until you move it for the first time. I consider this a
non-issue.

If you can't seem to get OSC to work, try one of these:

- Change the OSC port used by XSOverlay, look @ [their docs](https://xiexe.github.io/XSOverlayDocumentation/#/OSCAPI) (
  it does not use OSCQuery as of writing, so this is probably your issue)
- Reset your OSC config?

If this plugin's settings dont show up in the menu, it's likely:

- You are using a custom theme and it is conflicting somehow. As of writing this update is not fully out and i have no
  way of testing it lmfao
- Either your XSOverlay or Plugin are outdated
- something else, bug me about it or fix urself idk

If you still need help you can find me in my dev [discord](https://discord.gg/BrUacrw4cy)

## Build from source

Check the .csproj or actions workflow

but if you wanna build this just drop the necessary dlls from `XSOverlay_Data/Managed` into `refs`, restore and build w/
Release
config. dll will be in `builds` folder

### Motivation

- vrchat's keyboard is very buggy and likes to malfunction or just not work very often
- ovr toolkit has been hella laggy everytime i have come back to use it
- ur a nerd lol
