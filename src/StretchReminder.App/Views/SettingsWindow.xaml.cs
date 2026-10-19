using System.IO;
using System.Windows;
using Microsoft.Win32;
using StretchReminder.App.Services;
using StretchReminder.Core.Models;

namespace StretchReminder.App.Views;

public partial class SettingsWindow : Window
{
    private readonly ISettingsService _settingsService;
    private readonly ReminderTimerService _timerService;
    private string? _selectedVideoPath;

    public SettingsWindow(
        ISettingsService settingsService,
        ReminderTimerService timerService)
    {
        InitializeComponent();

        _settingsService = settingsService;
        _timerService = timerService;

        _timerService.OnTick += UpdateCountdownText;

        LoadCurrentSettings();
        UpdateCountdownText();
    }

    private void LoadCurrentSettings()
    {
        var s = _settingsService.CurrentSettings;

        IntervalSlider.Value = Math.Clamp(s.IntervalMinutes, 5, 180);
        IntervalValueText.Text = ((int)IntervalSlider.Value).ToString();

        MessageTextBox.Text = s.Message;

        _selectedVideoPath = s.VideoPath;
        VideoPathTextBox.Text = _selectedVideoPath ?? string.Empty;

        VolumeSlider.Value = (int)(s.VideoVolume * 100);
        VolumeText.Text = $"{(int)VolumeSlider.Value}%";

        MuteVideoCheckBox.IsChecked = s.MuteVideo;
        AutoStartCheckBox.IsChecked = s.AutoStartWithWindows;

        UpdateVideoPreview();
    }

    private void UpdateCountdownText()
    {
        Dispatcher.Invoke(() =>
        {
            var remaining = _timerService.TimeRemaining;
            TimeRemainingText.Text = $"{remaining.Hours * 60 + remaining.Minutes} min {remaining.Seconds:D2} sek";
            TimeRemainingText.Foreground = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(52, 211, 153)
            );
        });
    }

    private void IntervalSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (IntervalValueText != null)
        {
            IntervalValueText.Text = ((int)e.NewValue).ToString();
        }
    }

    private void QuickInterval_Click(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement elem && elem.Tag is string tagStr && int.TryParse(tagStr, out int val))
        {
            IntervalSlider.Value = val;
        }
    }

    private void BrowseVideo_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Title = "Wybierz plik wideo do przypomnienia",
            Filter = "Pliki wideo (*.mp4;*.wmv;*.avi;*.webm;*.mov)|*.mp4;*.wmv;*.avi;*.webm;*.mov|Wszystkie pliki (*.*)|*.*",
            CheckFileExists = true
        };

        if (dialog.ShowDialog() == true)
        {
            _selectedVideoPath = dialog.FileName;
            VideoPathTextBox.Text = _selectedVideoPath;
            UpdateVideoPreview();
        }
    }

    private void ClearVideo_Click(object sender, RoutedEventArgs e)
    {
        _selectedVideoPath = null;
        VideoPathTextBox.Text = string.Empty;
        UpdateVideoPreview();
    }

    private void UpdateVideoPreview()
    {
        try
        {
            if (!string.IsNullOrEmpty(_selectedVideoPath) && File.Exists(_selectedVideoPath))
            {
                PreviewMediaElement.Source = new Uri(_selectedVideoPath, UriKind.Absolute);
                PreviewMediaElement.Volume = MuteVideoCheckBox.IsChecked == true ? 0 : VolumeSlider.Value / 100.0;
                PreviewMediaElement.IsMuted = MuteVideoCheckBox.IsChecked == true;
                PreviewMediaElement.Visibility = Visibility.Visible;
                NoVideoPreviewText.Visibility = Visibility.Collapsed;
                PreviewMediaElement.Play();
            }
            else
            {
                StopPreview();
                PreviewMediaElement.Visibility = Visibility.Collapsed;
                NoVideoPreviewText.Visibility = Visibility.Visible;
            }
        }
        catch
        {
            StopPreview();
            PreviewMediaElement.Visibility = Visibility.Collapsed;
            NoVideoPreviewText.Visibility = Visibility.Visible;
        }
    }

    private void StopPreview()
    {
        try
        {
            PreviewMediaElement.Stop();
            PreviewMediaElement.Close();
        }
        catch
        {
        }
    }

    private void PreviewMediaElement_MediaEnded(object sender, RoutedEventArgs e)
    {
        PreviewMediaElement.Position = TimeSpan.Zero;
        PreviewMediaElement.Play();
    }

    private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (VolumeText != null)
        {
            VolumeText.Text = $"{(int)e.NewValue}%";
            if (PreviewMediaElement != null && MuteVideoCheckBox?.IsChecked != true)
            {
                PreviewMediaElement.Volume = e.NewValue / 100.0;
            }
        }
    }

    private void MuteVideoCheckBox_Changed(object sender, RoutedEventArgs e)
    {
        if (PreviewMediaElement != null)
        {
            bool isMuted = MuteVideoCheckBox.IsChecked == true;
            PreviewMediaElement.IsMuted = isMuted;
            PreviewMediaElement.Volume = isMuted ? 0 : VolumeSlider.Value / 100.0;
        }
    }

    private void TestReminder_Click(object sender, RoutedEventArgs e)
    {
        _timerService.TriggerNow();
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        var settings = new ReminderSettings
        {
            IntervalMinutes = (int)IntervalSlider.Value,
            Message = MessageTextBox.Text.Trim(),
            VideoPath = string.IsNullOrWhiteSpace(_selectedVideoPath) ? null : _selectedVideoPath,
            VideoVolume = VolumeSlider.Value / 100.0,
            MuteVideo = MuteVideoCheckBox.IsChecked == true,
            AutoStartWithWindows = AutoStartCheckBox.IsChecked == true
        };

        _settingsService.SaveSettings(settings);
        _timerService.Reset();

        StopPreview();
        Hide();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        StopPreview();
        Hide();
    }

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        e.Cancel = true;
        StopPreview();
        Hide();
    }
}
