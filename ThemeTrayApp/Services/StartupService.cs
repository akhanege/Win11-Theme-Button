using Microsoft.Win32;

namespace ThemeTrayApp.Services;

internal sealed class StartupService
{
    private const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string AppName = "ThemeTrayApp";

    public bool TryIsEnabled(out bool enabled)
    {
        enabled = false;

        try
        {
            using RegistryKey? key = Registry.CurrentUser.OpenSubKey(RunKeyPath, writable: false);
            string? command = key?.GetValue(AppName) as string;
            enabled = !string.IsNullOrWhiteSpace(command);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public bool TrySetEnabled(bool enabled, out string? error)
    {
        error = null;

        try
        {
            using RegistryKey key = Registry.CurrentUser.CreateSubKey(RunKeyPath, writable: true)
                ?? throw new InvalidOperationException("Unable to open run registry key.");

            if (enabled)
            {
                string executable = Environment.ProcessPath
                    ?? throw new InvalidOperationException("Executable path could not be determined.");
                key.SetValue(AppName, Quote(executable), RegistryValueKind.String);
            }
            else
            {
                key.DeleteValue(AppName, throwOnMissingValue: false);
            }

            return true;
        }
        catch (Exception ex)
        {
            error = ex.Message;
            return false;
        }
    }

    private static string Quote(string value) => $"\"{value}\"";

}
