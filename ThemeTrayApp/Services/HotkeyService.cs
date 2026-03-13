using System.Windows.Forms;

namespace ThemeTrayApp.Services;

[Flags]
internal enum KeyModifiers : uint
{
    None = 0,
    Alt = 0x0001,
    Control = 0x0002,
    Shift = 0x0004,
    Win = 0x0008
}

internal sealed class HotkeyService : IDisposable
{
    private const int DefaultHotkeyId = 0x2101;
    private readonly HotkeyWindow _window = new();
    private int _hotkeyId;

    public event EventHandler? HotkeyPressed;

    public HotkeyService()
    {
        _window.HotkeyPressed += (_, id) =>
        {
            if (id == _hotkeyId)
            {
                HotkeyPressed?.Invoke(this, EventArgs.Empty);
            }
        };
    }

    public bool TryRegisterDefault(out string? error)
    {
        return TryRegister(KeyModifiers.Control | KeyModifiers.Alt, Keys.T, out error);
    }

    public bool TryRegister(KeyModifiers modifiers, Keys key, out string? error)
    {
        error = null;
        Unregister();

        int id = DefaultHotkeyId;
        bool success = NativeMethods.RegisterHotKey(_window.Handle, id, (uint)modifiers, (uint)key);

        if (!success)
        {
            error = "Global hotkey kaydedilemedi (Ctrl+Alt+T kullanýlýyor olabilir).";
            return false;
        }

        _hotkeyId = id;
        return true;
    }

    public void Unregister()
    {
        if (_hotkeyId != 0)
        {
            _ = NativeMethods.UnregisterHotKey(_window.Handle, _hotkeyId);
            _hotkeyId = 0;
        }
    }

    public void Dispose()
    {
        Unregister();
        _window.Dispose();
    }

    private sealed class HotkeyWindow : NativeWindow, IDisposable
    {
        public event EventHandler<int>? HotkeyPressed;

        public HotkeyWindow()
        {
            CreateHandle(new CreateParams());
        }

        protected override void WndProc(ref Message m)
        {
            if ((uint)m.Msg == NativeMethods.WM_HOTKEY)
            {
                HotkeyPressed?.Invoke(this, m.WParam.ToInt32());
            }

            base.WndProc(ref m);
        }

        public void Dispose()
        {
            DestroyHandle();
        }
    }
}