using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Keystrokes
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowThreadProcessId(IntPtr hWnd, out uint ProcessId);

        public bool IsOpen { get; private set; }
        public ObservableCollection<string> ProcessItems { get; set; }

        private DispatcherTimer timer;

        private ProcessFilter processFilter;

        public SettingsWindow(ProcessFilter processFilter)
        {
            this.processFilter = processFilter;
            ProcessItems = new ObservableCollection<string>();
            InitializeComponent();

            IsOpen = true;

            timer = new DispatcherTimer();
            timer.Tick += Timer_Tick; ;
            timer.Interval = TimeSpan.FromMilliseconds(1000);

            this.ProcessListView.ItemsSource = ProcessItems;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            var hwnd = GetForegroundWindow();
            GetWindowThreadProcessId(hwnd, out uint pid);
            var process = Process.GetProcessById((int)pid);

            if (process.ProcessName != "Keystrokes")
            {
                ProcessTextBox.Text = process.ProcessName;
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            IsOpen = false;
            timer.Stop();

            if (this.FilterCheckBox.IsChecked.HasValue)
            {
                processFilter.IsEnable = this.FilterCheckBox.IsChecked.Value;
            }
            processFilter.ProcessItems.Clear();
            foreach (var processItem in this.ProcessItems)
            {
                processFilter.ProcessItems.Add(processItem);
            }

            this.processFilter.Save();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            timer.Start();

            this.FilterCheckBox.IsChecked = processFilter.IsEnable;
            this.ProcessItems.Clear();
            foreach (var processItem in processFilter.ProcessItems)
            {
                this.ProcessItems.Add(processItem);
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var processName = this.ProcessTextBox.Text.Trim();
            if (string.IsNullOrEmpty(processName) == false)
            {
                if (ProcessItems.Contains(processName) == false)
                {
                    ProcessItems.Add(processName);
                }
            }
            this.ProcessTextBox.Text = string.Empty;
        }

        private void DelButton_Click(object sender, RoutedEventArgs e)
        {
            var processName = this.ProcessTextBox.Text.Trim();
            if (ProcessItems.Contains(processName))
            {
                ProcessItems.Remove(processName);
            }
            this.ProcessTextBox.Text = string.Empty;
        }

        private void ProcessListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ProcessListView.SelectedItem != null)
            {
                this.ProcessTextBox.Text = ProcessListView.SelectedItem.ToString();
            }
        }
    }
}
