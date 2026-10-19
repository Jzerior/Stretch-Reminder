using System.Globalization;
using System.Resources;

namespace StretchReminder.Core.Localization;

public static class Strings
{
    private static readonly ResourceManager ResourceManager =
        new("StretchReminder.Core.Resources.Strings", typeof(Strings).Assembly);

    public static CultureInfo Culture { get; set; } = CultureInfo.CurrentUICulture;

    public static void Initialize(CultureInfo? culture = null)
    {
        Culture = culture ?? CultureInfo.CurrentUICulture;
    }

    public static string Get(string key) => ResourceManager.GetString(key, Culture) ?? key;

    public static string AppName => Get(nameof(AppName));
    public static string AppDescription => Get(nameof(AppDescription));
    public static string AlreadyRunning => Get(nameof(AlreadyRunning));
    public static string DefaultReminderMessage => Get(nameof(DefaultReminderMessage));

    public static string TrayNotificationStarted => Get(nameof(TrayNotificationStarted));
    public static string TraySettings => Get(nameof(TraySettings));
    public static string TrayRemindNow => Get(nameof(TrayRemindNow));
    public static string TraySnooze1Hour => Get(nameof(TraySnooze1Hour));
    public static string TrayResetTimer => Get(nameof(TrayResetTimer));
    public static string TrayExit => Get(nameof(TrayExit));
    public static string TrayTooltipPaused => Get(nameof(TrayTooltipPaused));
    public static string TrayTooltipNext(int minutes) => string.Format(Get("TrayTooltipNext"), minutes);

    public static string SettingsWindowTitle => Get(nameof(SettingsWindowTitle));
    public static string NextReminderIn => Get(nameof(NextReminderIn));
    public static string CountdownPaused => Get(nameof(CountdownPaused));
    public static string CountdownFormat(int minutes, int seconds) => string.Format(Get("CountdownFormat"), minutes, seconds);

    public static string IntervalHeader => Get(nameof(IntervalHeader));
    public static string IntervalDescription => Get(nameof(IntervalDescription));
    public static string QuickSelect => Get(nameof(QuickSelect));
    public static string DefaultInterval => Get(nameof(DefaultInterval));
    public static string MinutesSuffix => Get(nameof(MinutesSuffix));

    public static string MessageHeader => Get(nameof(MessageHeader));
    public static string MessageDescription => Get(nameof(MessageDescription));

    public static string VideoHeader => Get(nameof(VideoHeader));
    public static string VideoDescription => Get(nameof(VideoDescription));
    public static string BrowseVideo => Get(nameof(BrowseVideo));
    public static string ClearVideo => Get(nameof(ClearVideo));
    public static string VideoDialogTitle => Get(nameof(VideoDialogTitle));
    public static string VideoDialogFilter => Get(nameof(VideoDialogFilter));
    public static string NoVideoPreview => Get(nameof(NoVideoPreview));
    public static string MuteVideo => Get(nameof(MuteVideo));
    public static string VideoVolume => Get(nameof(VideoVolume));

    public static string SystemSettingsHeader => Get(nameof(SystemSettingsHeader));
    public static string AutoStartTitle => Get(nameof(AutoStartTitle));
    public static string AutoStartDescription => Get(nameof(AutoStartDescription));

    public static string TestReminderButton => Get(nameof(TestReminderButton));
    public static string CancelButton => Get(nameof(CancelButton));
    public static string SaveButton => Get(nameof(SaveButton));

    public static string ReminderTag => Get(nameof(ReminderTag));
    public static string ReminderTitle => Get(nameof(ReminderTitle));
    public static string DoneButton => Get(nameof(DoneButton));
    public static string SnoozeButton(int minutes) => string.Format(Get("SnoozeButton"), minutes);
}
