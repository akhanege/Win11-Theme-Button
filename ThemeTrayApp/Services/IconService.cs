using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;

namespace ThemeTrayApp.Services;

internal sealed class IconService : IDisposable
{
    private readonly Icon _lightIcon;
    private readonly Icon _darkIcon;

    public IconService()
    {
        _lightIcon = CreateLightIcon();
        _darkIcon = CreateDarkIcon();
    }

    public Icon GetIcon(ThemeMode mode) => mode == ThemeMode.Dark ? _darkIcon : _lightIcon;

    private static Icon CreateLightIcon()
    {
        using var bmp = new Bitmap(32, 32);
        using Graphics g = Graphics.FromImage(bmp);
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.Clear(Color.Transparent);

        using var sunBrush = new SolidBrush(Color.FromArgb(255, 250, 190, 60));
        using var rayPen = new Pen(Color.FromArgb(255, 245, 170, 20), 2);

        g.FillEllipse(sunBrush, 9, 9, 14, 14);

        PointF center = new(16, 16);
        for (int i = 0; i < 8; i++)
        {
            double angle = i * Math.PI / 4.0;
            float x1 = center.X + (float)(Math.Cos(angle) * 10);
            float y1 = center.Y + (float)(Math.Sin(angle) * 10);
            float x2 = center.X + (float)(Math.Cos(angle) * 14);
            float y2 = center.Y + (float)(Math.Sin(angle) * 14);
            g.DrawLine(rayPen, x1, y1, x2, y2);
        }

        return IconFromBitmap(bmp) ?? SystemIcons.Application;
    }

    private static Icon CreateDarkIcon()
    {
        Icon? emojiIcon = TryCreateEmojiIcon("🌙");
        if (emojiIcon is not null)
        {
            return emojiIcon;
        }

        return CreateDarkFallbackIcon();
    }

    private static Icon CreateDarkFallbackIcon()
    {
        using var bmp = new Bitmap(32, 32);
        using Graphics g = Graphics.FromImage(bmp);
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.Clear(Color.Transparent);

        using var moonBrush = new SolidBrush(Color.FromArgb(255, 210, 220, 240));
        using var cutBrush = new SolidBrush(Color.Transparent);

        g.FillEllipse(moonBrush, 7, 6, 18, 20);

        g.CompositingMode = CompositingMode.SourceCopy;
        g.FillEllipse(cutBrush, 13, 6, 14, 20);

        g.CompositingMode = CompositingMode.SourceOver;
        using var starBrush = new SolidBrush(Color.FromArgb(220, 245, 245, 255));
        g.FillEllipse(starBrush, 21, 8, 3, 3);
        g.FillEllipse(starBrush, 24, 13, 2, 2);

        return IconFromBitmap(bmp) ?? SystemIcons.Shield;
    }

    private static Icon? TryCreateEmojiIcon(string emoji)
    {
        try
        {
            using var bmp = new Bitmap(32, 32);
            using Graphics g = Graphics.FromImage(bmp);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
            g.Clear(Color.Transparent);

            using var font = new Font("Segoe UI Emoji", 21f, FontStyle.Regular, GraphicsUnit.Pixel);
            SizeF size = g.MeasureString(emoji, font);
            float x = (bmp.Width - size.Width) / 2f;
            float y = (bmp.Height - size.Height) / 2f - 1f;

            using var brush = new SolidBrush(Color.White);
            g.DrawString(emoji, font, brush, x, y);

            return IconFromBitmap(bmp);
        }
        catch
        {
            return null;
        }
    }

    private static Icon? IconFromBitmap(Bitmap bitmap)
    {
        IntPtr handle = bitmap.GetHicon();

        try
        {
            using Icon temp = Icon.FromHandle(handle);
            return (Icon)temp.Clone();
        }
        finally
        {
            _ = NativeMethods.DestroyIcon(handle);
        }
    }

    public void Dispose()
    {
        _lightIcon.Dispose();
        _darkIcon.Dispose();
    }
}
