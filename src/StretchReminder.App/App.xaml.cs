using System.Threading;
using System.Windows;
using StretchReminder.App.Services;
using StretchReminder.App.Views;
using StretchReminder.App.Localization;

namespace StretchReminder.App;

public partial class App : Application
{
    private Mutex? _mutex;
    private const string AppMutexName = "StretchReminder_SingleInstance_Mutex_99812";

    private SettingsService? _settingsService;
    private PluginManager? _pluginManager;
    private ReminderTimerService? _timerService;
    private TrayIconManager? _trayIconManager;

    private SettingsWindow? _settingsWindow;
    private ReminderWindow? _activeReminderWindow;

    protected override void OnStartup(StartupEventArgs e)
    {
        Strings.Initialize(System.Globalization.CultureInfo.CurrentUICulture);

        _mutex = new Mutex(true, AppMutexName, out bool createdNew);
        if (!createdNew)
        {
            MessageBox.Show(
                Strings.AlreadyRunning,
                Strings.AppName,
                MessageBoxButton.OK,
                MessageBoxImage.Information
            );
            Shutdown();
            return;
        }

        base.OnStartup(e);

        _settingsService = new SettingsService();
        _pluginManager = new PluginManager();
        _pluginManager.LoadPlugins();

        _timerService = new ReminderTimerService(_settingsService, _pluginManager);
        _timerService.OnReminderDue += TimerService_OnReminderDue;
        _timerService.OnTick += TimerService_OnTick;

        _trayIconManager = new TrayIconManager(
            openSettingsAction: ShowSettingsWindow,
            triggerNowAction: () => _timerService.TriggerNow(),
            snoozeAction: min => _timerService.Snooze(min),
            resetAction: () => _timerService.Reset(),
            exitAction: ExitApplication
        );

        _trayIconManager.ShowNotification(
            Strings.AppName,
            Strings.TrayNotificationStarted,
            System.Windows.Forms.ToolTipIcon.Info
        );
    }

    private void TimerService_OnTick()
    {
        if (_timerService == null || _trayIconManager == null) return;

        if (_timerService.IsPostponed)
        {
            _trayIconManager.UpdateTooltip(Strings.TrayTooltipPaused);
        }
        else
        {
            var remaining = _timerService.TimeRemaining;
            int totalMinutes = (int)Math.Ceiling(remaining.TotalMinutes);
            _trayIconManager.UpdateTooltip(Strings.TrayTooltipNext(totalMinutes));
        }
    }

    private void TimerService_OnReminderDue()
    {
        Dispatcher.Invoke(() =>
        {
            if (_settingsService == null || _timerService == null) return;

            if (_activeReminderWindow != null && _activeReminderWindow.IsVisible)
            {
                _activeReminderWindow.Activate();
                return;
            }

            _activeReminderWindow = new ReminderWindow(_settingsService.CurrentSettings);
            _activeReminderWindow.OnDone += () =>
            {
                _timerService.Reset();
                _pluginManager?.NotifyReminderDismissed();
                _activeReminderWindow = null;
            };
            _activeReminderWindow.OnSnooze += (minutes) =>
            {
                _timerService.Snooze(minutes);
                _activeReminderWindow = null;
            };

            _pluginManager?.NotifyReminderTriggered();
            _activeReminderWindow.Show();
            _activeReminderWindow.Activate();
        });
    }

    private void ShowSettingsWindow()
    {
        if (_settingsService == null || _timerService == null) return;

        if (_settingsWindow == null)
        {
            _settingsWindow = new SettingsWindow(_settingsService, _timerService);
        }

        _settingsWindow.Show();
        _settingsWindow.WindowState = WindowState.Normal;
        _settingsWindow.Activate();
    }

    private void ExitApplication()
    {
        _trayIconManager?.Dispose();
        _timerService?.Stop();
        _pluginManager?.UnloadAll();

        _mutex?.ReleaseMutex();
        _mutex?.Dispose();

        Shutdown();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _trayIconManager?.Dispose();
        _timerService?.Stop();
        _pluginManager?.UnloadAll();

        _mutex?.ReleaseMutex();
        _mutex?.Dispose();

        base.OnExit(e);
    }
}
