using System.Threading;
using ThemeTrayApp.Services;
using ThemeTrayApp.UI;

namespace ThemeTrayApp;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        using var mutex = new Mutex(initiallyOwned: true, "Global\\ThemeTrayApp_SingleInstance_Mutex", out bool isFirstInstance);
        if (!isFirstInstance)
        {
            return;
        }

        ApplicationConfiguration.Initialize();

        var themeService = new ThemeService();
        var startupService = new StartupService();
        using var hotkeyService = new HotkeyService();
        using var iconService = new IconService();
        using var appContext = new TrayApplicationContext(themeService, startupService, hotkeyService, iconService);

        Application.Run(appContext);
    }
}
