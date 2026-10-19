using StretchReminder.Core.Localization;

namespace StretchReminder.Core.Models;

public class ReminderSettings
{
    public int IntervalMinutes { get; set; } = 60;
    public string Message { get; set; } = Strings.DefaultReminderMessage;
    public string? VideoPath { get; set; }
    public double VideoVolume { get; set; } = 0.5;
    public bool MuteVideo { get; set; } = false;
    public int SnoozeMinutes { get; set; } = 5;
    public bool AutoStartWithWindows { get; set; } = false;
    public bool PostponeIfGameDetected { get; set; } = true;
    public List<string> EnabledPlugins { get; set; } = new();
}
