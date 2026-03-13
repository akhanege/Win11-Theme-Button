using ThemeTrayApp.Services;

namespace ThemeTrayApp.UI;

internal sealed class TrayApplicationContext : ApplicationContext
{
    private readonly ThemeService _themeService;
    private readonly StartupService _startupService;
    private readonly HotkeyService _hotkeyService;
    private readonly IconService _iconService;

    private readonly NotifyIcon _notifyIcon;
    private readonly ContextMenuStrip _contextMenu;
    private readonly ToolStripMenuItem _startWithWindowsItem;

    private ThemeMode _currentMode = ThemeMode.Light;

    public TrayApplicationContext(
        ThemeService themeService,
        StartupService startupService,
        HotkeyService hotkeyService,
        IconService iconService)
    {
        _themeService = themeService;
        _startupService = startupService;
        _hotkeyService = hotkeyService;
        _iconService = iconService;

        _startWithWindowsItem = new ToolStripMenuItem("Start with Windows")
        {
            CheckOnClick = true
        };
        _startWithWindowsItem.Click += (_, _) => ToggleStartup(_startWithWindowsItem.Checked);

        _contextMenu = new ContextMenuStrip();
        _contextMenu.Items.Add("Toggle Theme", null, (_, _) => ToggleTheme());
        _contextMenu.Items.Add("Light Mode", null, (_, _) => SetTheme(ThemeMode.Light));
        _contextMenu.Items.Add("Dark Mode", null, (_, _) => SetTheme(ThemeMode.Dark));
        _contextMenu.Items.Add(new ToolStripSeparator());
        _contextMenu.Items.Add(_startWithWindowsItem);
        _contextMenu.Items.Add(new ToolStripSeparator());
        _contextMenu.Items.Add("Exit", null, (_, _) => ExitThread());

        _notifyIcon = new NotifyIcon
        {
            Text = "Theme Tray App",
            Visible = true,
            ContextMenuStrip = _contextMenu
        };
        _notifyIcon.MouseUp += NotifyIconOnMouseUp;

        InitializeState();
        RegisterHotkey();
    }

    private void InitializeState()
    {
        if (!_themeService.TryGetCurrentTheme(out _currentMode))
        {
            _currentMode = ThemeMode.Light;
            ShowInfo("Tema bilgisi okunamadi. Varsayilan ikon kullaniliyor.");
        }

        ApplyIcon();

        if (_startupService.TryIsEnabled(out bool isEnabled))
        {
            _startWithWindowsItem.Checked = isEnabled;
        }
    }

    private void RegisterHotkey()
    {
        _hotkeyService.HotkeyPressed += (_, _) => ToggleTheme();
        if (!_hotkeyService.TryRegisterDefault(out string? error) && !string.IsNullOrWhiteSpace(error))
        {
            ShowInfo(error);
        }
    }

    private void NotifyIconOnMouseUp(object? sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            ToggleTheme();
        }
    }

    private void ToggleTheme()
    {
        if (_themeService.TryToggleTheme(out ThemeMode newMode, out string? error))
        {
            _currentMode = newMode;
            ApplyIcon();
            return;
        }

        ShowInfo($"Tema degistirilemedi: {error ?? "Bilinmeyen hata"}");
    }

    private void SetTheme(ThemeMode mode)
    {
        if (_themeService.TrySetTheme(mode, out string? error))
        {
            _currentMode = mode;
            ApplyIcon();
            return;
        }

        ShowInfo($"Tema ayarlanamadi: {error ?? "Bilinmeyen hata"}");
    }

    private void ToggleStartup(bool enabled)
    {
        if (_startupService.TrySetEnabled(enabled, out string? error))
        {
            return;
        }

        _startWithWindowsItem.Checked = !enabled;
        ShowInfo($"Start with Windows guncellenemedi: {error ?? "Bilinmeyen hata"}");
    }

    private void ApplyIcon()
    {
        _notifyIcon.Icon = _iconService.GetIcon(_currentMode);
        _notifyIcon.Text = _currentMode == ThemeMode.Dark
            ? "Theme Tray App (Dark)"
            : "Theme Tray App (Light)";
    }

    private void ShowInfo(string message)
    {
        _notifyIcon.BalloonTipTitle = "Theme Tray App";
        _notifyIcon.BalloonTipText = message;
        _notifyIcon.ShowBalloonTip(2500);
    }

    protected override void ExitThreadCore()
    {
        _notifyIcon.MouseUp -= NotifyIconOnMouseUp;
        _notifyIcon.Visible = false;
        _notifyIcon.Dispose();
        _contextMenu.Dispose();

        _hotkeyService.Unregister();

        base.ExitThreadCore();
    }
}
