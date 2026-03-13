using Microsoft.Win32;

namespace ThemeTrayApp.Services;

internal sealed class ThemeService
{
    private const string PersonalizeKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";
    private const string AppsUseLightTheme = "AppsUseLightTheme";
    private const string SystemUsesLightTheme = "SystemUsesLightTheme";

    public bool TryGetCurrentTheme(out ThemeMode mode)
    {
        mode = ThemeMode.Light;

        try
        {
            using RegistryKey? key = Registry.CurrentUser.OpenSubKey(PersonalizeKeyPath, writable: false);
            if (key is null)
            {
                return true;
            }

            int apps = ReadDwordValue(key, AppsUseLightTheme, defaultValue: 1);
            int system = ReadDwordValue(key, SystemUsesLightTheme, defaultValue: 1);

            mode = (apps == 0 || system == 0) ? ThemeMode.Dark : ThemeMode.Light;
            return true;
        }
        catch
        {
            return false;
        }
    }

    public bool TrySetTheme(ThemeMode mode, out string? error)
    {
        error = null;

        try
        {
            using RegistryKey key = Registry.CurrentUser.CreateSubKey(PersonalizeKeyPath, writable: true)
                ?? throw new InvalidOperationException("Unable to open personalize registry key.");

            int value = mode == ThemeMode.Light ? 1 : 0;
            key.SetValue(AppsUseLightTheme, value, RegistryValueKind.DWord);
            key.SetValue(SystemUsesLightTheme, value, RegistryValueKind.DWord);

            BroadcastThemeChanged();
            return true;
        }
        catch (Exception ex)
        {
            error = ex.Message;
            return false;
        }
    }

    public bool TryToggleTheme(out ThemeMode newMode, out string? error)
    {
        error = null;
        newMode = ThemeMode.Light;

        if (!TryGetCurrentTheme(out ThemeMode current))
        {
            error = "Current theme could not be read.";
            return false;
        }

        newMode = current == ThemeMode.Light ? ThemeMode.Dark : ThemeMode.Light;
        return TrySetTheme(newMode, out error);
    }

    private static int ReadDwordValue(RegistryKey key, string name, int defaultValue)
    {
        object? value = key.GetValue(name, defaultValue);

        return value switch
        {
            int intValue => intValue,
            byte byteValue => byteValue,
            _ => defaultValue
        };
    }

    private static void BroadcastThemeChanged()
    {
        _ = NativeMethods.SendMessageTimeout(
            NativeMethods.HwndBroadcast,
            NativeMethods.WM_SETTINGCHANGE,
            UIntPtr.Zero,
            "ImmersiveColorSet",
            NativeMethods.SendMessageTimeoutFlags.AbortIfHung,
            2000,
            out _);

        _ = NativeMethods.SendMessageTimeout(
            NativeMethods.HwndBroadcast,
            NativeMethods.WM_THEMECHANGED,
            UIntPtr.Zero,
            null,
            NativeMethods.SendMessageTimeoutFlags.AbortIfHung,
            2000,
            out _);
    }

}
