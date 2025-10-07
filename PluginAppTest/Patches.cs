using HarmonyLib;
using XSOverlay;

namespace PluginAppTest;

public static class Patches
{
    private static Harmony _harmony;

    public static void PatchAll()
    {
        _harmony = new Harmony("nwnt.keyboardosc");

        var create = AccessTools.Method(typeof(PluginApp), "CreateApplicationWindow", [typeof(AppInfo)]);
        var patch = new HarmonyMethod(typeof(Patches).GetMethod(nameof(CreatePluginPatch)));
        _harmony.Patch(create, null, patch);

        var setSettings = AccessTools.Method(typeof(XSettingsManager), nameof(XSettingsManager.SetSetting));
        var setSettingsPatch = new HarmonyMethod(typeof(Patches).GetMethod(nameof(SetSettingPatch)));

        _harmony.Patch(setSettings, setSettingsPatch);
    }

    public static void CreatePluginPatch(AppInfo appInfo)
    {
        Plugin.PluginLogger.LogInfo(
            $"Creating application window for {appInfo.Author} {appInfo.Name} {appInfo.Version}!");
        Overlay_Manager.Instance.CreateNewOverlayWindow(false);
        var overlays = Overlay_Manager.Instance.AllSceneOverlays;
        var pluginOverlay = overlays[overlays.Count];
        pluginOverlay.IsPluginApplication = true;
        pluginOverlay.widthInMeters = appInfo.DefaultScale;
        pluginOverlay.renderTexWidthOverride = appInfo.ResWidth;
        pluginOverlay.renderTexHeightOverride = appInfo.ResHeight;
        pluginOverlay.WebViewHandler.WebView.LoadUrl(appInfo.URL);
        Overlay_Manager.Instance.AppDrawerOverlay.transform.parent.gameObject.SetActive(false);
    }

    public static bool SetSettingPatch(string name, string value, string value1, bool sendAnalytics = false)
    {
        switch (name)
        {
            case "OpenPluginList":
                Plugin.PluginLogger.LogInfo("guh");
                Plugin.PluginLogger.LogInfo($"{Overlay_Manager.Instance.AppDrawerOverlay.name}");
                Overlay_Manager.Instance.AppDrawerOverlay.transform.parent.gameObject.SetActive(true);
                CreatePluginPatch(new AppInfo
                {
                    Author = "guhg", Name = "Twitch",
                    Version = "v1.0",
                    Description = "Twitch for XSOverlay :)",
                    URL = "https://www.twitch.tv/",
                    ResWidth = 1920,
                    ResHeight = 1080,
                    IconColor = [0.56862745098f, 0.27450980392f, 1.00000000000f],
                    DefaultScale = 0.2f,
                    CanOpenMultiple = true,
                    CanCurve = false
                });
                break;
        }

        return true;
    }
}