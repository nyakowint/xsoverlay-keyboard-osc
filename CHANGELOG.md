
This version includes fixes for Build 680 and is not backward compatible.

How many people use this? Let me know with a emoji reaction below or something. seems like an unpopular github feature thats why im using it also i was curious


- The install script now handles install, uninstall, and updating the plugin!
- Enabled the console window for all new plugin installs. This could help with random errors that pop up after updates.
    - if you want to enable it for your install, you can find it in `BepInEx\config\BepInEx.cfg`
    - or replace yours w the one in this release
- Simplified version and update checking. 
  - In short we don't compare entire assemblies anymore lol. 
  - should also make the download count accurate because there AINT NO WAY that 11thousand people dl'd the last release lmao
- Hopefully fixed the chatbox popping sound being sent with every chatbox update? 
  - If you experience this happening with KBChat still please create an issue!
- The "Disable max length" setting now actually works... how did i miss that all this time

- The keyboard is getting an webview(?) update next so prepare for things to be broken again :P
    - The next plugin update may take longer due to this
- Twitch integration had some issues that would take longer than a day to fix so it was temporarily(?) removed.
  - i am only one person (kinda). the less things to maintain the better
  - if there is enough interest it will return sooner. otherwise it will be reworked at some point

this list is not exhaustive but i think i got everything.

If you experience any issues please first ensure they are not an XSOverlay bug by removing KeyboardOSC!
After doing that if the issue persists create a discussion post (for problems/support) or an issue on this repo.

[Install Script](https://github.com/nyakowint/xsoverlay-keyboard-osc#how-to-install)