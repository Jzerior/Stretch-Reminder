# Stretch Reminder (.NET 9 / WPF)

A Windows desktop utility running in the background system tray that regularly reminds you to stand up from your desk, stretch, and stay active.

---

## Features

- **System Tray Background Operation**:
  - Runs unobtrusively in the notification area (system tray).
  - Left-click or double-click opens the Settings window.
  - Right-click opens a context menu (Settings, Remind Now, Snooze for 1 hour, Reset Timer, Exit).
  - Dynamic tooltip indicates the remaining time until the next reminder.

- **Settings Panel**:
  - **Interval Configuration**: Slider and quick-selection buttons (30 min, 45 min, default 60 min, 90 min, 120 min).
  - **Custom Notification Message**: Option to set any personalized reminder text.
  - **Background Video**: Select local video files (.mp4, .wmv, .webm, .avi) with an inline live preview, volume slider, and mute toggle.
  - **Autostart with Windows**: Toggle to start the application automatically on system login.
  - **Testing**: "Test reminder now" button to preview the popup window and video immediately.

- **Popup Reminder Window**:
  - Centered overlay window displayed on top of other applications (`Topmost = true`).
  - Seamlessly loops selected background video (or displays an elegant gradient fallback when no video is selected).
  - Quick action buttons: "Done! I stretched" (resets the timer) and "Snooze for 5 min".

- **Localization (Resource-Based i18n)**:
  - Automatically matches the Windows operating system UI language (`CultureInfo.CurrentUICulture`).
  - Pre-packaged with English (`Strings.resx`) and Polish (`Strings.pl.resx`).
  - Extensible: Add a new `.resx` file (such as `Strings.de.resx` or `Strings.es.resx`) to the `Resources/` folder, and `ResourceManager` will load it automatically without any code modifications.

- **Modular Architecture**:
  - Shared class library `StretchReminder.Core` contains standard plugin interfaces (`IReminderPlugin`, `IActivityDetector`, `IPluginContext`), enabling external integrations (such as online match detection, webhooks, or third-party notifications) from separate repositories.

---

## Project Structure

```
Stretch-Reminder/
├── StretchReminder.sln
├── src/
│   ├── StretchReminder.Core/               # Interfaces, models, and localization resources
│   │   ├── Models/
│   │   │   └── ReminderSettings.cs
│   │   ├── Plugins/
│   │   │   ├── IReminderPlugin.cs
│   │   │   ├── IActivityDetector.cs
│   │   │   └── IPluginContext.cs
│   │   ├── Localization/
│   │   │   └── Strings.cs                  # ResourceManager facade
│   │   └── Resources/
│   │       ├── Strings.resx                # Default resources (English)
│   │       └── Strings.pl.resx             # Polish resources
│   │
│   └── StretchReminder.App/                # Main WPF Application (.NET 9)
│       ├── Services/
│       │   ├── SettingsService.cs          # JSON configuration and registry autostart
│       │   ├── ReminderTimerService.cs     # Countdown timer and scheduling
│       │   ├── TrayIconManager.cs          # System tray icon and context menu
│       │   ├── PluginManager.cs            # Dynamic assembly loader
│       │   └── IconHelper.cs               # Vector tray icon generation
│       └── Views/
│           ├── SettingsWindow.xaml         # Settings window
│           └── ReminderWindow.xaml         # Popup reminder window
```

---

## Getting Started

### Prerequisites:
- Windows 10 or Windows 11
- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)

### Build and Run:
```powershell
dotnet build
dotnet run --project src/StretchReminder.App
```

### Publish as a Single-File Executable (.exe):
```powershell
dotnet publish src/StretchReminder.App/StretchReminder.App.csproj `
  -c Release `
  -r win-x64 `
  --self-contained true `
  -p:PublishSingleFile=true `
  -o ./publish
```
The standalone executable `StretchReminder.App.exe` will be generated in `./publish`.
Exe file will be published here when first release-ready version will be reached