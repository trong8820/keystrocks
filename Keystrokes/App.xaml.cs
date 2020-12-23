using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Keystrokes
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private SettingsWindow settingsWindow;
        private MainWindow mainwindow;

        private readonly System.Windows.Forms.NotifyIcon notifyIcon;
        private readonly System.Windows.Forms.MenuItem lockMenuItem;

        private bool isOn = true;
        private ProcessFilter processFilter;

        public App()
        {
            processFilter = new ProcessFilter();
            settingsWindow = new SettingsWindow(processFilter);
            mainwindow = new MainWindow(processFilter);
            mainwindow.Show();

            notifyIcon = new System.Windows.Forms.NotifyIcon
            {
                Visible = true,
                Icon = Keystrokes.Properties.Resources.icon_on
            };
            notifyIcon.MouseClick += NotifyIcon_MouseClick;
            notifyIcon.MouseDoubleClick += NotifyIcon_MouseDoubleClick;

            System.Windows.Forms.ContextMenu notifyContextMenu = new System.Windows.Forms.ContextMenu();
            notifyContextMenu.MenuItems.Add("Settings", new EventHandler(OnSettings));
            lockMenuItem = notifyContextMenu.MenuItems.Add("Lock", new EventHandler(OnLock));
            notifyContextMenu.MenuItems.Add("-");
            notifyContextMenu.MenuItems.Add("Exit", new EventHandler(OnExit));

            notifyIcon.ContextMenu = notifyContextMenu;
        }

        private void OnSettings(object sender, EventArgs e)
        {
            if (settingsWindow.IsOpen == false)
            {
                settingsWindow = new SettingsWindow(processFilter);
            }
            settingsWindow.Show();
        }

        private void OnLock(object sender, EventArgs e)
        {
            lockMenuItem.Checked = !lockMenuItem.Checked;
            mainwindow.DragArea.Visibility = lockMenuItem.Checked ? Visibility.Hidden : Visibility.Visible;
        }

        private void OnExit(object sender, EventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void NotifyIcon_MouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                isOn = !isOn;
                notifyIcon.Icon = isOn ? Keystrokes.Properties.Resources.icon_on : Keystrokes.Properties.Resources.icon_off;
                mainwindow.Visibility = isOn ? Visibility.Visible : Visibility.Hidden;
            }
        }

        private void NotifyIcon_MouseDoubleClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (settingsWindow.IsOpen == false)
            {
                settingsWindow = new SettingsWindow(processFilter);
            }
            settingsWindow.Show();
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            notifyIcon.Visible = false;
        }
    }
}
