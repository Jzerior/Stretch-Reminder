using System.Drawing;
using System.Drawing.Drawing2D;

namespace StretchReminder.App.Services;

public static class IconHelper
{
    public static Icon CreateAppIcon()
    {
        const int size = 32;
        using var bitmap = new Bitmap(size, size);
        using var g = Graphics.FromImage(bitmap);

        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.PixelOffsetMode = PixelOffsetMode.HighQuality;

        using var brush = new LinearGradientBrush(
            new Rectangle(0, 0, size, size),
            Color.FromArgb(16, 185, 129),
            Color.FromArgb(14, 165, 233),
            LinearGradientMode.ForwardDiagonal
        );
        g.FillEllipse(brush, 2, 2, size - 4, size - 4);

        using var pen = new Pen(Color.White, 1.5f);
        g.DrawEllipse(pen, 2, 2, size - 4, size - 4);

        using var whiteBrush = new SolidBrush(Color.White);
        using var figurePen = new Pen(Color.White, 2.2f)
        {
            StartCap = LineCap.Round,
            EndCap = LineCap.Round
        };

        g.FillEllipse(whiteBrush, 13, 6, 6, 6);
        g.DrawLine(figurePen, 16, 12, 16, 21);
        g.DrawLine(figurePen, 8, 11, 16, 15);
        g.DrawLine(figurePen, 24, 11, 16, 15);
        g.DrawLine(figurePen, 16, 21, 11, 27);
        g.DrawLine(figurePen, 16, 21, 21, 27);

        IntPtr hIcon = bitmap.GetHicon();
        return (Icon)Icon.FromHandle(hIcon).Clone();
    }
}
