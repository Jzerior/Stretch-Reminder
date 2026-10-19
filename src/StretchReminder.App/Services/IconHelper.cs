using System.Drawing;
using System.IO;
using System.Windows;

namespace StretchReminder.App.Services;

public static class IconHelper
{
    private static readonly Uri IconUri = new("pack://application:,,,/Resources/app.ico", UriKind.Absolute);

    public static Icon CreateAppIcon()
    {
        try
        {
            var streamInfo = Application.GetResourceStream(IconUri);
            if (streamInfo != null)
            {
                using var stream = streamInfo.Stream;
                return new Icon(stream, 32, 32);
            }
        }
        catch
        {
        }

        string fallbackPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "app.ico");
        if (File.Exists(fallbackPath))
        {
            using var stream = File.OpenRead(fallbackPath);
            return new Icon(stream, 32, 32);
        }

        return SystemIcons.Application;
    }
}
