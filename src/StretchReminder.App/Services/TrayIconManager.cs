using System.Drawing;
using System.Windows.Forms;
using StretchReminder.App.Localization;

namespace StretchReminder.App.Services;

public class TrayIconManager : IDisposable
{
    private readonly NotifyIcon _notifyIcon;
    private readonly Action _openSettingsAction;
    private readonly Action _triggerNowAction;
    private readonly Action<int> _snoozeAction;
    private readonly Action _resetAction;
    private readonly Action _exitAction;

    public TrayIconManager(
        Action openSettingsAction,
        Action triggerNowAction,
        Action<int> snoozeAction,
        Action resetAction,
        Action exitAction)
    {
        _openSettingsAction = openSettingsAction;
        _triggerNowAction = triggerNowAction;
        _snoozeAction = snoozeAction;
        _resetAction = resetAction;
        _exitAction = exitAction;

        _notifyIcon = new NotifyIcon
        {
            Icon = IconHelper.CreateAppIcon(),
            Text = Strings.AppName,
            Visible = true
        };

        SetupContextMenu();

        _notifyIcon.DoubleClick += (s, e) => _openSettingsAction();
        _notifyIcon.Click += (s, e) =>
        {
            if (e is MouseEventArgs me && me.Button == MouseButtons.Left)
            {
                _openSettingsAction();
            }
        };
    }

    private void SetupContextMenu()
    {
        var contextMenu = new ContextMenuStrip();

        var openItem = new ToolStripMenuItem(Strings.TraySettings, null, (s, e) => _openSettingsAction());
        openItem.Font = new Font(openItem.Font, FontStyle.Bold);

        var triggerNowItem = new ToolStripMenuItem(Strings.TrayRemindNow, null, (s, e) => _triggerNowAction());
        var snooze1hItem = new ToolStripMenuItem(Strings.TraySnooze1Hour, null, (s, e) => _snoozeAction(60));
        var resetItem = new ToolStripMenuItem(Strings.TrayResetTimer, null, (s, e) => _resetAction());

        var exitItem = new ToolStripMenuItem(Strings.TrayExit, null, (s, e) => _exitAction());

        contextMenu.Items.Add(openItem);
        contextMenu.Items.Add(new ToolStripSeparator());
        contextMenu.Items.Add(triggerNowItem);
        contextMenu.Items.Add(snooze1hItem);
        contextMenu.Items.Add(resetItem);
        contextMenu.Items.Add(new ToolStripSeparator());
        contextMenu.Items.Add(exitItem);

        _notifyIcon.ContextMenuStrip = contextMenu;
    }

    public void UpdateTooltip(string text)
    {
        if (text.Length > 63)
        {
            text = text.Substring(0, 60) + "...";
        }
        _notifyIcon.Text = text;
    }

    public void ShowNotification(string title, string message, ToolTipIcon icon = ToolTipIcon.Info)
    {
        _notifyIcon.ShowBalloonTip(3000, title, message, icon);
    }

    public void Dispose()
    {
        _notifyIcon.Visible = false;
        _notifyIcon.Icon?.Dispose();
        _notifyIcon.ContextMenuStrip?.Dispose();
        _notifyIcon.Dispose();
    }
}
