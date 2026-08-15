Version 2.0.0 is a rewrite for XSOverlay's new web based keyboard (beta 688+).

The Unity keyboard the old chatbar was built on top of is gone, so the chatbox now lives inside
the keyboard page itself:

- The chat bar is the keyboard's own text field. Hit the chat bubble button on the left of it to
  take over the keyboard, hit it again (or ESC on an empty bar) to hand it back.
- The chat button decides where the bar sends, whatever wrote into it. That includes
  XSOverlay's dictation: with chat mode on, a transcription goes to the chatbox instead of being
  typed into Windows (which VRChat ignores anyway). With chat mode off the microphone behaves
  exactly like stock.
- Text macros got a menu on the right end of the chat bar (same dropdown the keyboard uses for
  its speech language), so you don't have to remember the shortcodes. Typing them still works.
- The keyboard's copy and paste keys act on the chat bar. They do nothing in the base build,
  XSOverlay ships them without a keycode.
- Fixed `//hrt2` and `//skull2` expanding as `//hrt` + "2" / `//skull` + "2".
- Plugin settings moved into XSOverlay's own Keyboard settings page as a "Chatbox" section,
  instead of adding a separate sidebar page.
- Typing no longer reaches Windows while chat mode is on, because XSOverlay routes the keys to
  the chat bar itself. The input blocking hack from 1.x is gone.
- END only clears the chatbox when the bar is empty (it moves the caret otherwise).

Older versions of the plugin do not work on beta 688 and newer. This version does not work on
older builds either.

---

Version 1.3.1 fixes a bug with adaptive media theme colors not applying.

Version 1.3.0 changes how Keyboard OSC settings are added to the settings page & how patches are applied internally. If you encounter any problems with this update please create an issue!

Update notes:

Patch Settings.html using WebView.ExecuteJavaScript (#7) \
Thanks @chaixshot for bringing my attention to this issue!

---
They have created [xsoverlay-font-changer](https://github.com/chaixshot/xsoverlay-font-changer),
which can fix certain keyboard layouts appearing as boxes in the unity-based keyboard overlay.

you could also use it for aesthetic purposes. probably. idk im not a qa tester.

A quick-install for it has also been added to the [install script](https://github.com/nyakowint/xsoverlay-keyboard-osc#install)!
