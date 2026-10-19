using System.Windows.Threading;
using StretchReminder.Core.Models;

namespace StretchReminder.App.Services;

public class ReminderTimerService
{
    private readonly ISettingsService _settingsService;
    private readonly PluginManager? _pluginManager;
    private readonly DispatcherTimer _timer;

    private DateTime _nextReminderTime;
    private bool _isPostponed;
    private string? _postponeReason;

    public TimeSpan TimeRemaining
    {
        get
        {
            var remaining = _nextReminderTime - DateTime.Now;
            return remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
        }
    }

    public DateTime NextReminderTime => _nextReminderTime;
    public bool IsPostponed => _isPostponed;
    public string? PostponeReason => _postponeReason;

    public event Action? OnReminderDue;
    public event Action? OnTick;

    public ReminderTimerService(ISettingsService settingsService, PluginManager? pluginManager = null)
    {
        _settingsService = settingsService;
        _pluginManager = pluginManager;

        _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _timer.Tick += Timer_Tick;

        Reset();
    }

    public void Start()
    {
        if (!_timer.IsEnabled)
        {
            _timer.Start();
        }
    }

    public void Stop()
    {
        if (_timer.IsEnabled)
        {
            _timer.Stop();
        }
    }

    public void Reset()
    {
        int minutes = Math.Max(1, _settingsService.CurrentSettings.IntervalMinutes);
        _nextReminderTime = DateTime.Now.AddMinutes(minutes);
        _isPostponed = false;
        _postponeReason = null;
        Start();
    }

    public void Snooze(int? minutes = null)
    {
        int snoozeMin = minutes ?? Math.Max(1, _settingsService.CurrentSettings.SnoozeMinutes);
        _nextReminderTime = DateTime.Now.AddMinutes(snoozeMin);
        _isPostponed = false;
        _postponeReason = null;
        _pluginManager?.NotifyReminderSnoozed(snoozeMin);
        Start();
    }

    public void TriggerNow()
    {
        _isPostponed = false;
        _postponeReason = null;
        OnReminderDue?.Invoke();
    }

    private void Timer_Tick(object? sender, EventArgs e)
    {
        OnTick?.Invoke();

        if (DateTime.Now >= _nextReminderTime)
        {
            if (_pluginManager != null && _settingsService.CurrentSettings.PostponeIfGameDetected)
            {
                foreach (var detector in _pluginManager.ActivityDetectors)
                {
                    if (detector.ShouldPostponeReminder(out string? reason))
                    {
                        _isPostponed = true;
                        _postponeReason = reason;
                        _nextReminderTime = DateTime.Now.AddSeconds(30);
                        return;
                    }
                }
            }

            _isPostponed = false;
            _postponeReason = null;
            Stop();
            OnReminderDue?.Invoke();
        }
    }
}
