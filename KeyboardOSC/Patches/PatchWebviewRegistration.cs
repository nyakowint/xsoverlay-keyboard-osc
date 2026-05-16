using System;
using HarmonyLib;
using Vuplex.WebView;
using XSOverlay;

namespace KeyboardOSC.Patches;


[HarmonyPatch(typeof(Overlay_Manager), nameof(Overlay_Manager.ToggleApplicationSettings))]
internal static class PatchWebviewRegistration
{
    [HarmonyPostfix]
    public static void Postfix(Overlay_Manager __instance)
    {
        if (!__instance.DashboardSettingsOverlay.activeSelf) return;

        try
        {
            var webView = __instance.GlobalSettingsMenuOverlay?.OverlayWebView?._webView?.WebView;
            if (webView == null)
            {
                Plugin.PluginLogger.LogWarning("[KBOSC:Patches] Settings webview is null — skipping injection");
                return;
            }

            InjectSettingsModule(webView);
        }
        catch (Exception ex)
        {
            Plugin.PluginLogger.LogError($"[KBOSC:Patches] Exception injecting settings module: {ex}");
        }
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
            s.src = './_Shared/js/settings-chatbox.js';
            document.head.appendChild(s);
            return 'injected';
        })();";

        webView.ExecuteJavaScript(js, result =>
            Plugin.PluginLogger.LogInfo($"[KBOSC:Patches] Result of settings injection: {result}"));
    }
}
