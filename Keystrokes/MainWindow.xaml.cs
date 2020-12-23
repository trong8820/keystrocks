using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;

namespace Keystrokes
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HT_CAPTION = 0x2;

        private KeyboardHook keyboardHook;
        private MouseHook mouseHook;

        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowThreadProcessId(IntPtr hWnd, out uint ProcessId);
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();
        [DllImportAttribute("user32.dll")]
        private static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImportAttribute("user32.dll")]
        private static extern bool ReleaseCapture();

        private ProcessFilter processFilter;
        private bool isProcessActive;

        // Resource
        private SolidColorBrush backgroundBrush;
        private SolidColorBrush borderBrush;
        private SolidColorBrush activeBrush;
        private SolidColorBrush doubleBrush;

        // Timer
        private DispatcherTimer keyLargeTimer;
        private DispatcherTimer keyShortTimer;
        private DispatcherTimer mouseWheelTimer;
        private DispatcherTimer mouseMoveTimer;
        private DispatcherTimer processFilterTimer;

        public MainWindow(ProcessFilter processFilter)
        {
            this.processFilter = processFilter;
            InitializeComponent();

            backgroundBrush = (SolidColorBrush)FindResource("BackgroundBrush");
            borderBrush = (SolidColorBrush)FindResource("BorderBrush");
            activeBrush = (SolidColorBrush)FindResource("ActiveBrush");
            doubleBrush = (SolidColorBrush)FindResource("DoubleBrush");

            // Timer
            keyLargeTimer = new DispatcherTimer();
            keyLargeTimer.Tick += KeyLargeTimer_Tick;
            keyLargeTimer.Interval = TimeSpan.FromMilliseconds(500);

            keyShortTimer = new DispatcherTimer();
            keyShortTimer.Tick += KeyShortTimer_Tick;
            keyShortTimer.Interval = TimeSpan.FromMilliseconds(1000);

            mouseWheelTimer = new DispatcherTimer();
            mouseWheelTimer.Tick += MouseWheelTimer_Tick;
            mouseWheelTimer.Interval = TimeSpan.FromMilliseconds(200);

            mouseMoveTimer = new DispatcherTimer();
            mouseMoveTimer.Tick += MouseMoveTimer_Tick;
            mouseMoveTimer.Interval = TimeSpan.FromMilliseconds(200);

            processFilterTimer = new DispatcherTimer();
            processFilterTimer.Tick += ProcessFilterTimer_Tick; ;
            processFilterTimer.Interval = TimeSpan.FromMilliseconds(1000);
            processFilterTimer.Start();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            // Keyboard
            keyboardHook = new KeyboardHook();
            keyboardHook.OnKeyPressed += KeyboardHook_OnKeyPressed;
            keyboardHook.OnKeyUp += KeyboardHook_OnKeyUp;
            keyboardHook.HookKeyboard();

            // Mouse
            mouseHook = new MouseHook();
            mouseHook.OnMousePressed += MouseHook_OnMousePressed;
            mouseHook.OnMouseUp += MouseHook_OnMouseUp;
            mouseHook.OnDoubleClick += MouseHook_OnDoubleClick;
            mouseHook.OnMouseWheel += MouseHook_OnMouseWheel;
            mouseHook.OnMouseMove += MouseHook_OnMouseMove;
            mouseHook.HookMouse();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            keyboardHook.UnHookKeyboard();
            mouseHook.UnHookMouse();
        }

        // Keyboard
        private void KeyboardHook_OnKeyPressed(object sender, KeyArgs e)
        {
            if (isProcessActive == false) return;

            //Console.WriteLine($"KeyPressed {process.ProcessName} {e.Key}");
            // Ctrl
            if (e.Key == Key.LeftCtrl)
            {
                this.CtrlLeftUI.Fill = activeBrush;
            }
            if (e.Key == Key.RightCtrl)
            {
                this.CtrlRightUI.Fill = activeBrush;
            }
            if (this.CtrlLeftUI.Fill == activeBrush || this.CtrlRightUI.Fill == activeBrush)
            {
                this.CtrlUI.Foreground = activeBrush;
            }

            // Shift
            if (e.Key == Key.LeftShift)
            {
                this.ShiftLeftUI.Fill = activeBrush;
            }
            if (e.Key == Key.RightShift)
            {
                this.ShiftRightUI.Fill = activeBrush;
            }
            if (this.ShiftLeftUI.Fill == activeBrush || this.ShiftRightUI.Fill == activeBrush)
            {
                this.ShiftUI.Fill = activeBrush;
            }

            // Alt
            if (e.Key == Key.LeftAlt)
            {
                this.AltLeftUI.Fill = activeBrush;
            }
            if (e.Key == Key.RightAlt)
            {
                this.AltRightUI.Fill = activeBrush;
            }
            if (this.AltLeftUI.Fill == activeBrush || this.AltRightUI.Fill == activeBrush)
            {
                this.AltUI.Foreground = activeBrush;
            }

            // Large key
            if (e.Key.ToString().Length > 1)
            {
                if (e.Key != Key.LeftCtrl && e.Key != Key.RightCtrl &&
                    e.Key != Key.LeftShift && e.Key != Key.RightShift &&
                    e.Key != Key.LeftAlt && e.Key != Key.RightAlt)
                {
                    this.KeyLargeUI.Content = e.Key.ToString();

                    keyLargeTimer.Stop();
                    keyLargeTimer.Start();
                }
            }

            // Short key
            if (e.Key.ToString().Length == 1)
            {
                if (string.IsNullOrEmpty(this.KeyShort1UI.Content.ToString()))
                {
                    this.KeyShort1UI.Content = e.Key.ToString();
                } else if (string.IsNullOrEmpty(this.KeyShort2UI.Content.ToString()))
                {
                    if (this.KeyShort1UI.Content.ToString() != e.Key.ToString())
                    {
                        this.KeyShort2UI.Content = e.Key.ToString();
                    }
                } else
                {
                    if (this.KeyShort2UI.Content.ToString() != e.Key.ToString())
                    {
                        this.KeyShort1UI.Content = this.KeyShort2UI.Content;
                        this.KeyShort2UI.Content = e.Key.ToString();
                    }
                }

                keyShortTimer.Stop();
                keyShortTimer.Start();
            }
        }

        private void KeyboardHook_OnKeyUp(object sender, KeyArgs e)
        {
            if (isProcessActive == false) return;
            //Console.WriteLine($"KeyUp {e.Key}");

            // Ctrl
            if (e.Key == Key.LeftCtrl)
            {
                this.CtrlLeftUI.Fill = backgroundBrush;
            }
            if (e.Key == Key.RightCtrl)
            {
                this.CtrlRightUI.Fill = backgroundBrush;
            }
            if (this.CtrlLeftUI.Fill == backgroundBrush && this.CtrlRightUI.Fill == backgroundBrush)
            {
                this.CtrlUI.Foreground = borderBrush;
            }

            // Shift
            if (e.Key == Key.LeftShift)
            {
                this.ShiftLeftUI.Fill = backgroundBrush;
            }
            if (e.Key == Key.RightShift)
            {
                this.ShiftRightUI.Fill = backgroundBrush;
            }
            if (this.ShiftLeftUI.Fill == backgroundBrush && this.ShiftRightUI.Fill == backgroundBrush)
            {
                this.ShiftUI.Fill = borderBrush;
            }

            // Alt
            if (e.Key == Key.LeftAlt)
            {
                this.AltLeftUI.Fill = backgroundBrush;
            }
            if (e.Key == Key.RightAlt)
            {
                this.AltRightUI.Fill = backgroundBrush;
            }
            if (this.AltLeftUI.Fill == backgroundBrush && this.AltRightUI.Fill == backgroundBrush)
            {
                this.AltUI.Foreground = borderBrush;
            }
        }


        private void KeyLargeTimer_Tick(object sender, EventArgs e)
        {
            (sender as DispatcherTimer).Stop();

            this.KeyLargeUI.Content = string.Empty;
        }

        private void KeyShortTimer_Tick(object sender, EventArgs e)
        {
            (sender as DispatcherTimer).Stop();

            this.KeyShort1UI.Content = string.Empty;
            this.KeyShort2UI.Content = string.Empty;
        }

        // Mouse
        private void MouseHook_OnMousePressed(object sender, MouseArgs e)
        {
            if (isProcessActive == false) return;
            switch (e.MouseButton)
            {
                case MouseButton.Left:
                    this.MouseLeftUI.Fill = activeBrush;
                    break;
                case MouseButton.Middle:
                    this.MouseMidleUI.Fill = activeBrush;
                    break;
                case MouseButton.Right:
                    this.MouseRightUI.Fill = activeBrush;
                    break;
                case MouseButton.XButton1:
                    break;
                case MouseButton.XButton2:
                    break;
                default:
                    break;
            }
        }

        private void MouseHook_OnMouseUp(object sender, MouseArgs e)
        {
            if (isProcessActive == false) return;
            switch (e.MouseButton)
            {
                case MouseButton.Left:
                    this.MouseLeftUI.Fill = backgroundBrush;
                    break;
                case MouseButton.Middle:
                    this.MouseMidleUI.Fill = backgroundBrush;
                    break;
                case MouseButton.Right:
                    this.MouseRightUI.Fill = backgroundBrush;
                    break;
                case MouseButton.XButton1:
                    break;
                case MouseButton.XButton2:
                    break;
                default:
                    break;
            }
        }

        private void MouseHook_OnDoubleClick(object sender, MouseArgs e)
        {
            if (isProcessActive == false) return;
            switch (e.MouseButton)
            {
                case MouseButton.Left:
                    this.MouseLeftUI.Fill = doubleBrush;
                    break;
                case MouseButton.Middle:
                    this.MouseMidleUI.Fill = doubleBrush;
                    break;
                case MouseButton.Right:
                    this.MouseRightUI.Fill = doubleBrush;
                    break;
                case MouseButton.XButton1:
                    break;
                case MouseButton.XButton2:
                    break;
                default:
                    break;
            }
        }

        private void MouseHook_OnMouseWheel(object sender, EventArgs e)
        {
            if (isProcessActive == false) return;
            this.MouseWheelUI.Visibility = Visibility.Visible;

            mouseWheelTimer.Stop();
            mouseWheelTimer.Start();
        }

        private void MouseWheelTimer_Tick(object sender, EventArgs e)
        {
            (sender as DispatcherTimer).Stop();

            this.MouseWheelUI.Visibility = Visibility.Hidden;
        }

        private void MouseHook_OnMouseMove(object sender, EventArgs e)
        {
            if (isProcessActive == false) return;
            this.MouseMoveUI.Fill = activeBrush;

            mouseMoveTimer.Stop();
            mouseMoveTimer.Start();
        }

        private void MouseMoveTimer_Tick(object sender, EventArgs e)
        {
            (sender as DispatcherTimer).Stop();

            this.MouseMoveUI.Fill = backgroundBrush;
        }

        private void DragArea_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (isProcessActive == false) return;
            ReleaseCapture();
            var window = Window.GetWindow(this);
            var wih = new WindowInteropHelper(window);
            SendMessage(wih.EnsureHandle(), WM_NCLBUTTONDOWN, HT_CAPTION, 0);
        }

        private void ProcessFilterTimer_Tick(object sender, EventArgs e)
        {
            if (processFilter.IsEnable)
            {
                var hwnd = GetForegroundWindow();
                GetWindowThreadProcessId(hwnd, out uint pid);
                var process = Process.GetProcessById((int)pid);
                if (processFilter.ProcessItems.Contains(process.ProcessName) == false)
                {
                    isProcessActive = false;
                    return;
                }
            }
            isProcessActive = true;
        }
    }
}
