namespace StretchReminder.Core.Plugins;

public interface IActivityDetector : IReminderPlugin
{
    bool ShouldPostponeReminder(out string? reason);
}
