namespace StretchReminder.Core.Plugins;

public interface IPluginContext
{
    string PluginStorageDirectory { get; }

    event Action? OnReminderTriggered;
    event Action? OnReminderDismissed;
    event Action<int>? OnReminderSnoozed;

    void LogInfo(string message);
    void LogWarning(string message);
    void LogError(string message, Exception? exception = null);
}
