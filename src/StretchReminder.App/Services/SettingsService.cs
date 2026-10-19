using System.IO;
using System.Text.Json;
using Microsoft.Win32;
using StretchReminder.Core.Models;

namespace StretchReminder.App.Services;

public interface ISettingsService
{
    ReminderSettings CurrentSettings { get; }
    void LoadSettings();
    void SaveSettings(ReminderSettings settings);
    bool SetAutoStart(bool enable);
    bool IsAutoStartEnabled();
}

public class SettingsService : ISettingsService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    private readonly string _settingsFolder;
    private readonly string _settingsFilePath;
    private const string AppName = "StretchReminder";

    public ReminderSettings CurrentSettings { get; private set; } = new();

    public SettingsService()
    {
        _settingsFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "StretchReminder"
        );
        _settingsFilePath = Path.Combine(_settingsFolder, "settings.json");

        LoadSettings();
    }

    public void LoadSettings()
    {
        try
        {
            if (File.Exists(_settingsFilePath))
            {
                string json = File.ReadAllText(_settingsFilePath);
                var loaded = JsonSerializer.Deserialize<ReminderSettings>(json, JsonOptions);
                if (loaded != null)
                {
                    CurrentSettings = loaded;
                    CurrentSettings.AutoStartWithWindows = IsAutoStartEnabled();
                    return;
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Błąd wczytywania ustawień: {ex.Message}");
        }

        CurrentSettings = new ReminderSettings();
        CurrentSettings.AutoStartWithWindows = IsAutoStartEnabled();
    }

    public void SaveSettings(ReminderSettings settings)
    {
        CurrentSettings = settings;

        try
        {
            if (!Directory.Exists(_settingsFolder))
            {
                Directory.CreateDirectory(_settingsFolder);
            }

            string json = JsonSerializer.Serialize(settings, JsonOptions);
            File.WriteAllText(_settingsFilePath, json);

            SetAutoStart(settings.AutoStartWithWindows);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Błąd zapisywania ustawień: {ex.Message}");
        }
    }

    public bool IsAutoStartEnabled()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", false);
            return key?.GetValue(AppName) != null;
        }
        catch
        {
            return false;
        }
    }

    public bool SetAutoStart(bool enable)
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);
            if (key == null) return false;

            if (enable)
            {
                string? exePath = Environment.ProcessPath;
                if (!string.IsNullOrEmpty(exePath))
                {
                    key.SetValue(AppName, $"\"{exePath}\"");
                    return true;
                }
            }
            else
            {
                key.DeleteValue(AppName, false);
                return true;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Błąd konfiguracji autostartu: {ex.Message}");
        }

        return false;
    }
}
