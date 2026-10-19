using System.IO;
using System.Windows;
using StretchReminder.Core.Localization;
using StretchReminder.Core.Models;

namespace StretchReminder.App.Views;

public partial class ReminderWindow : Window
{
    private readonly ReminderSettings _settings;
    public event Action? OnDone;
    public event Action<int>? OnSnooze;

    public ReminderWindow(ReminderSettings settings)
    {
        InitializeComponent();
        _settings = settings;

        ApplySettings();
    }

    private void ApplySettings()
    {
        MessageTextBlock.Text = !string.IsNullOrWhiteSpace(_settings.Message)
            ? _settings.Message
            : Strings.DefaultReminderMessage;

        SnoozeButton.Content = Strings.SnoozeButton(_settings.SnoozeMinutes);
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        PlayVideoIfAvailable();
    }

    private void PlayVideoIfAvailable()
    {
        if (!string.IsNullOrEmpty(_settings.VideoPath) && File.Exists(_settings.VideoPath))
        {
            try
            {
                BackgroundVideo.Source = new Uri(_settings.VideoPath, UriKind.Absolute);
                BackgroundVideo.Volume = _settings.MuteVideo ? 0.0 : Math.Clamp(_settings.VideoVolume, 0.0, 1.0);
                BackgroundVideo.IsMuted = _settings.MuteVideo;
                BackgroundVideo.Visibility = Visibility.Visible;
                FallbackBackground.Visibility = Visibility.Collapsed;
                BackgroundVideo.Play();
                return;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Błąd odtwarzania wideo: {ex.Message}");
            }
        }

        BackgroundVideo.Visibility = Visibility.Collapsed;
        FallbackBackground.Visibility = Visibility.Visible;
    }

    private void BackgroundVideo_MediaEnded(object sender, RoutedEventArgs e)
    {
        BackgroundVideo.Position = TimeSpan.Zero;
        BackgroundVideo.Play();
    }

    private void BackgroundVideo_MediaFailed(object? sender, ExceptionRoutedEventArgs e)
    {
        BackgroundVideo.Visibility = Visibility.Collapsed;
        FallbackBackground.Visibility = Visibility.Visible;
    }

    private void DoneButton_Click(object sender, RoutedEventArgs e)
    {
        StopVideo();
        OnDone?.Invoke();
        Close();
    }

    private void SnoozeButton_Click(object sender, RoutedEventArgs e)
    {
        StopVideo();
        OnSnooze?.Invoke(_settings.SnoozeMinutes);
        Close();
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        StopVideo();
        OnDone?.Invoke();
        Close();
    }

    private void StopVideo()
    {
        try
        {
            BackgroundVideo.Stop();
            BackgroundVideo.Close();
        }
        catch
        {
        }
    }

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        StopVideo();
    }
}
