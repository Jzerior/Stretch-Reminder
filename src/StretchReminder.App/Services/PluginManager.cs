using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using StretchReminder.Core.Plugins;

namespace StretchReminder.App.Services;

public class PluginManager : IPluginContext
{
    private readonly string _pluginsDirectory;
    private readonly string _pluginStorageRoot;
    private readonly List<IReminderPlugin> _loadedPlugins = new();

    public IReadOnlyList<IReminderPlugin> LoadedPlugins => _loadedPlugins.AsReadOnly();
    public IEnumerable<IActivityDetector> ActivityDetectors => _loadedPlugins.OfType<IActivityDetector>();

    public string PluginsDirectory => _pluginsDirectory;
    public string PluginStorageDirectory => _pluginStorageRoot;

    public event Action? OnReminderTriggered;
    public event Action? OnReminderDismissed;
    public event Action<int>? OnReminderSnoozed;

    public PluginManager()
    {
        _pluginsDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "plugins");
        _pluginStorageRoot = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "StretchReminder",
            "PluginStorage"
        );

        EnsureDirectories();
    }

    private void EnsureDirectories()
    {
        if (!Directory.Exists(_pluginsDirectory))
        {
            Directory.CreateDirectory(_pluginsDirectory);
        }
        if (!Directory.Exists(_pluginStorageRoot))
        {
            Directory.CreateDirectory(_pluginStorageRoot);
        }
    }

    public void LoadPlugins()
    {
        _loadedPlugins.Clear();
        EnsureDirectories();

        var dllFiles = Directory.GetFiles(_pluginsDirectory, "*.dll", SearchOption.AllDirectories);

        foreach (var dllPath in dllFiles)
        {
            string fileName = Path.GetFileName(dllPath);
            if (fileName.Equals("StretchReminder.Core.dll", StringComparison.OrdinalIgnoreCase))
                continue;

            try
            {
                LoadPluginAssembly(dllPath);
            }
            catch (Exception ex)
            {
                LogError($"Błąd ładowania wtyczki z {fileName}: {ex.Message}", ex);
            }
        }
    }

    private void LoadPluginAssembly(string dllPath)
    {
        var loadContext = new PluginLoadContext(dllPath);
        var assembly = loadContext.LoadFromAssemblyPath(dllPath);

        var pluginTypes = assembly.GetTypes()
            .Where(t => typeof(IReminderPlugin).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

        foreach (var type in pluginTypes)
        {
            try
            {
                if (Activator.CreateInstance(type) is IReminderPlugin plugin)
                {
                    plugin.Initialize(this);
                    _loadedPlugins.Add(plugin);
                }
            }
            catch (Exception ex)
            {
                LogError($"Błąd inicjalizacji wtyczki {type.FullName}: {ex.Message}", ex);
            }
        }
    }

    public void UnloadAll()
    {
        foreach (var plugin in _loadedPlugins)
        {
            try
            {
                plugin.Shutdown();
            }
            catch (Exception ex)
            {
                LogError($"Błąd zamykania wtyczki {plugin.Name}: {ex.Message}", ex);
            }
        }
        _loadedPlugins.Clear();
    }

    public void OpenPluginsFolder()
    {
        EnsureDirectories();
        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
        {
            FileName = _pluginsDirectory,
            UseShellExecute = true
        });
    }

    public void NotifyReminderTriggered() => OnReminderTriggered?.Invoke();
    public void NotifyReminderDismissed() => OnReminderDismissed?.Invoke();
    public void NotifyReminderSnoozed(int minutes) => OnReminderSnoozed?.Invoke(minutes);

    public void LogInfo(string message) => System.Diagnostics.Debug.WriteLine($"[INFO] {message}");
    public void LogWarning(string message) => System.Diagnostics.Debug.WriteLine($"[WARN] {message}");
    public void LogError(string message, Exception? exception = null) =>
        System.Diagnostics.Debug.WriteLine($"[ERROR] {message} {exception?.Message}");
}

public class PluginLoadContext : AssemblyLoadContext
{
    private readonly AssemblyDependencyResolver _resolver;

    public PluginLoadContext(string pluginPath) : base(isCollectible: true)
    {
        _resolver = new AssemblyDependencyResolver(pluginPath);
    }

    protected override Assembly? Load(AssemblyName assemblyName)
    {
        if (assemblyName.Name == "StretchReminder.Core")
        {
            return typeof(IReminderPlugin).Assembly;
        }

        string? assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);
        if (assemblyPath != null)
        {
            return LoadFromAssemblyPath(assemblyPath);
        }

        return null;
    }

    protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
    {
        string? libraryPath = _resolver.ResolveUnmanagedDllToPath(unmanagedDllName);
        if (libraryPath != null)
        {
            return LoadUnmanagedDllFromPath(libraryPath);
        }

        return IntPtr.Zero;
    }
}
