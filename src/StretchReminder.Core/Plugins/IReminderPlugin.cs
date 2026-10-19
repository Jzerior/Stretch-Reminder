namespace StretchReminder.Core.Plugins;

public interface IReminderPlugin
{
    string Id { get; }
    string Name { get; }
    string Description { get; }
    string Version { get; }
    string Author { get; }

    void Initialize(IPluginContext context);
    void Shutdown();
}
