using System;
using UnityEngine;
using Vuplex.WebView;
using XSOverlay;
using XSOverlay.WebApp;

namespace KeyboardOSC;

#pragma warning disable Publicizer001
/// <summary>
/// XSOverlay's keyboard and settings pages are both web apps now, so our additions are
/// script modules injected into them once they've finished loading.
/// </summary>
public static class UiInjector
{
    // Leading slash matters, WindowSettings.html is a different page
    private const string KeyboardPage = "/Keyboard.html";
    private const string SettingsPage = "/Settings.html";

    private static bool _subscribed;

    public static void Subscribe()
    {
        if (_subscribed) return;
        XSOEventSystem.OnInternalWebUIInitialized += OnWebUiInitialized;
        _subscribed = true;
    }

    public static void Unsubscribe()
    {
        if (!_subscribed) return;
        XSOEventSystem.OnInternalWebUIInitialized -= OnWebUiInitialized;
        _subscribed = false;
    }

    private static void OnWebUiInitialized(string url, bool success)
    {
        if (string.IsNullOrEmpty(url)) return;
        if (!success)
        {
            if (IsPatchablePage(url))
                Plugin.PluginLogger.LogWarning($"[KBOSC:Inject] {url} failed to load, nothing to inject into");
            return;
        }

        InjectInto(url);
    }

    /// <summary>Injects into any of our pages that are already loaded (plugin loaded late, etc).</summary>
    public static void InjectExisting()
    {
        foreach (var webView in Resources.FindObjectsOfTypeAll<OverlayWebView>())
        {
            var url = webView == null ? null : webView.LoadedURL;
            // Pages that haven't loaded yet get picked up by the event instead
            if (!string.IsNullOrEmpty(url) && IsPatchablePage(url)) InjectInto(url, true);
        }
    }

    private static bool IsPatchablePage(string url)
    {
        return url.EndsWith(KeyboardPage, StringComparison.OrdinalIgnoreCase) ||
               url.EndsWith(SettingsPage, StringComparison.OrdinalIgnoreCase);
    }

    private static void InjectInto(string url, bool quiet = false)
    {
        if (url.EndsWith(KeyboardPage, StringComparison.OrdinalIgnoreCase))
        {
            Inject(url, "chatbox-keyboard.js", "_kbosc_keyboard", quiet);
        }
        else if (url.EndsWith(SettingsPage, StringComparison.OrdinalIgnoreCase))
        {
            Inject(url, "chatbox-settings.js", "_kbosc_settings", quiet);
        }
    }

    private static void Inject(string url, string script, string marker, bool quiet)
    {
        try
        {
            var webView = FindWebView(url);
            if (webView == null)
            {
                if (!quiet)
                    Plugin.PluginLogger.LogWarning($"[KBOSC:Inject] Couldn't find a webview for {url} — skipping {script}");
                return;
            }

            if (!Plugin.ModifiedUiSuccess)
            {
                Plugin.PluginLogger.LogWarning($"[KBOSC:Inject] UI scripts weren't written to disk, skipping {script}");
                return;
            }

            Plugin.PluginLogger.LogInfo($"[KBOSC:Inject] Adding {script} to {url}");
            webView.ExecuteJavaScript(BuildInjection(script, marker),
                result => Plugin.PluginLogger.LogInfo($"[KBOSC:Inject] {script}: {result}"));
        }
        catch (Exception ex)
        {
            Plugin.PluginLogger.LogError($"[KBOSC:Inject] Exception injecting {script}: {ex}");
        }
    }

    // Pages only load once per launch, so a fresh key each run beats serving a cached script
    // from before the plugin was updated
    private static readonly string CacheKey = DateTime.UtcNow.Ticks.ToString();

    private static string BuildInjection(string script, string marker)
    {
        var cacheKey = CacheKey;
        return $@"(function() {{
            if (document.getElementById('{marker}')) return 'already injected';
            var m = document.createElement('div');
            m.id = '{marker}';
            m.style.display = 'none';
            document.body.appendChild(m);
            var s = document.createElement('script');
            s.type = 'module';
            s.src = './_Shared/js/{script}?v={cacheKey}';
            document.head.appendChild(s);
            return 'injected';
        }})();";
    }

    private static IWebView FindWebView(string url)
    {
        foreach (var overlayWebView in Resources.FindObjectsOfTypeAll<OverlayWebView>())
        {
            if (overlayWebView == null) continue;
            if (!string.Equals(overlayWebView.LoadedURL, url, StringComparison.OrdinalIgnoreCase)) continue;

            var webView = overlayWebView._webView?.WebView;
            if (webView is { IsDisposed: false }) return webView;
        }

        return null;
    }
}
