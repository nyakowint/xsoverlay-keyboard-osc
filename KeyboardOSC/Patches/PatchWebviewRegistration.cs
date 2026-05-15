using HarmonyLib;
using Vuplex.WebView;
using XSOverlay;
using XSOverlay.WebApp;

namespace KeyboardOSC.Patches;

[HarmonyPatch(typeof(Overlay_Manager), "OnRegisterWebviewOverlay")]
internal static class PatchWebviewRegistration
{
    [HarmonyPostfix]
    public static void Postfix(OverlayWebView wv)
    {
        if (!Plugin.ModifiedUiSuccess) return;
        if (wv.UserInterfaceSelection != OverlayWebView.UserInterfacePaths.Settings) return;

        Plugin.PluginLogger.LogInfo("[KBOSC:Patches] Settings webview registered — hooking LoadProgressChanged for KO injection");
        var webView = wv._webView.WebView;

        webView.LoadProgressChanged += (sender, args) =>
        {
            if (args.Type != ProgressChangeType.Finished) return;
            if (!webView.Url.Contains("Settings.html")) return;
            InjectSettingsModule(webView);
        };
    }

    private static void InjectSettingsModule(IWebView webView)
    {
        Plugin.PluginLogger.LogInfo("[KBOSC:Patches] Injecting settings page additions!");
        const string js = @"(function() {
            if (document.getElementById('_kbosc_injected')) return 'KbOSC already injected';
            var m = document.createElement('div');
            m.id = '_kbosc_injected';
            m.style.display = 'none';
            document.body.appendChild(m);
            var s = document.createElement('script');
            s.type = 'module';
            s.src = './_Shared/js/settingsKO.js';
            document.head.appendChild(s);
            return 'injected';
        })();";

        webView.ExecuteJavaScript(js, result =>
            Plugin.PluginLogger.LogInfo($"[KBOSC:Patches] Result of settings injection: {result}"));
    }
}
