using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Keystrokes
{
    [StructLayout(LayoutKind.Sequential)]
    struct Point
    {
        /// <summary>
        ///     Specifies the X-coordinate of the point.
        /// </summary>
        public int X;

        /// <summary>
        ///     Specifies the Y-coordinate of the point.
        /// </summary>
        public int Y;

        public Point(int x, int y)
        {
            X = x;
            Y = y;
        }

        public static bool operator ==(Point a, Point b)
        {
            return a.X == b.X && a.Y == b.Y;
        }

        public static bool operator !=(Point a, Point b)
        {
            return !(a == b);
        }

        public bool Equals(Point other)
        {
            return other.X == X && other.Y == Y;
        }

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (obj.GetType() != typeof(Point)) return false;
            return Equals((Point)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (X * 397) ^ Y;
            }
        }
    }

    [StructLayout(LayoutKind.Explicit)]
    struct MouseStruct
    {
        [FieldOffset(0x00)] public Point Point;
        [FieldOffset(0x0A)] public short MouseData;
        [FieldOffset(0x10)] public int Timestamp;
    }

    class MouseHook
    {
        private const int WH_MOUSE_LL = 14;

        private const int WM_LBUTTONDOWN = 0x0201;
        private const int WM_LBUTTONUP = 0x0202;
        private const int WM_LBUTTONDBLCLK = 0x0203;
        private const int WM_MBUTTONDOWN = 0x0207;
        private const int WM_MBUTTONUP = 0x0208;
        private const int WM_MBUTTONDBLCLK = 0x0209;
        private const int WM_RBUTTONDOWN = 0x0204;
        private const int WM_RBUTTONUP = 0x0205;
        private const int WM_RBUTTONDBLCLK = 0x0206;

        private const int WM_MOUSEMOVE = 0x0200;
        private const int WM_MOUSEWHEEL = 0x020A;

        [DllImport("user32", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetDoubleClickTime();

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, MouseHookProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        public delegate IntPtr MouseHookProc(int nCode, IntPtr wParam, IntPtr lParam);

        public event EventHandler<MouseArgs> OnMousePressed;
        public event EventHandler<MouseArgs> OnMouseUp;
        public event EventHandler<MouseArgs> OnDoubleClick;
        public event EventHandler OnMouseMove;
        public event EventHandler OnMouseWheel;

        private readonly MouseHookProc mouseHookProc;
        private IntPtr hHookID = IntPtr.Zero;

        // Double click
        private MouseButton previousClicked;
        private Point previousClickedPoint;
        private int clickedTime;

        public MouseHook()
        {
            mouseHookProc = HookCallback;
        }

        public void HookMouse()
        {
            hHookID = SetHook(mouseHookProc);
        }

        public void UnHookMouse()
        {
            UnhookWindowsHookEx(hHookID);
        }

        private IntPtr SetHook(MouseHookProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_MOUSE_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                if (wParam == (IntPtr)WM_LBUTTONDOWN)
                {
                    var point = ((MouseStruct)Marshal.PtrToStructure(lParam, typeof(MouseStruct))).Point;
                    var time = Environment.TickCount & int.MaxValue;
                    if (previousClicked == MouseButton.Left &&
                        previousClickedPoint == point &&
                        time > clickedTime &&
                        time - clickedTime <= GetDoubleClickTime())
                    {
                        OnDoubleClick?.Invoke(this, new MouseArgs(MouseButton.Left));
                        clickedTime = -1;
                    }
                    else
                    {
                        OnMousePressed?.Invoke(this, new MouseArgs(MouseButton.Left));

                        previousClicked = MouseButton.Left;
                        previousClickedPoint = point;
                        clickedTime = time;
                    }
                }

                if (wParam == (IntPtr)WM_LBUTTONUP)
                {
                    OnMouseUp?.Invoke(this, new MouseArgs(MouseButton.Left));

                    previousClicked = MouseButton.Left;
                    previousClickedPoint = ((MouseStruct)Marshal.PtrToStructure(lParam, typeof(MouseStruct))).Point;
                }

                /*if (wParam == (IntPtr)WM_LBUTTONDBLCLK)
                {
                    OnDoubleClick?.Invoke(this, new MouseArgs(MouseButton.Left));
                }*/

                if (wParam == (IntPtr)WM_MBUTTONDOWN)
                {
                    var point = ((MouseStruct)Marshal.PtrToStructure(lParam, typeof(MouseStruct))).Point;
                    var time = Environment.TickCount & int.MaxValue;
                    if (previousClicked == MouseButton.Middle &&
                        previousClickedPoint == point &&
                        time > clickedTime &&
                        time - clickedTime <= GetDoubleClickTime())
                    {
                        OnDoubleClick?.Invoke(this, new MouseArgs(MouseButton.Middle));
                        clickedTime = -1;
                    }
                    else
                    {
                        OnMousePressed?.Invoke(this, new MouseArgs(MouseButton.Middle));

                        previousClicked = MouseButton.Middle;
                        previousClickedPoint = point;
                        clickedTime = time;
                    }
                }

                if (wParam == (IntPtr)WM_MBUTTONUP)
                {
                    OnMouseUp?.Invoke(this, new MouseArgs(MouseButton.Middle));

                    previousClicked = MouseButton.Middle;
                    previousClickedPoint = ((MouseStruct)Marshal.PtrToStructure(lParam, typeof(MouseStruct))).Point;
                }

                /*if (wParam == (IntPtr)WM_MBUTTONDBLCLK)
                {
                    OnDoubleClick?.Invoke(this, new MouseArgs(MouseButton.Middle));
                }*/

                if (wParam == (IntPtr)WM_RBUTTONDOWN)
                {
                    var point = ((MouseStruct)Marshal.PtrToStructure(lParam, typeof(MouseStruct))).Point;
                    var time = Environment.TickCount & int.MaxValue;
                    if (previousClicked == MouseButton.Right &&
                        previousClickedPoint == point &&
                        time > clickedTime &&
                        time - clickedTime <= GetDoubleClickTime())
                    {
                        OnDoubleClick?.Invoke(this, new MouseArgs(MouseButton.Right));
                        clickedTime = -1;
                    }
                    else
                    {
                        OnMousePressed?.Invoke(this, new MouseArgs(MouseButton.Right));

                        previousClicked = MouseButton.Right;
                        previousClickedPoint = point;
                        clickedTime = time;
                    }
                }

                if (wParam == (IntPtr)WM_RBUTTONUP)
                {
                    OnMouseUp?.Invoke(this, new MouseArgs(MouseButton.Right));

                    previousClicked = MouseButton.Right;
                    previousClickedPoint = ((MouseStruct)Marshal.PtrToStructure(lParam, typeof(MouseStruct))).Point;
                }

                /*if (wParam == (IntPtr)WM_RBUTTONDBLCLK)
                {
                    OnDoubleClick?.Invoke(this, new MouseArgs(MouseButton.Right));
                }*/

                if (wParam == (IntPtr)WM_MOUSEMOVE)
                {
                    OnMouseMove?.Invoke(this, new EventArgs());
                }

                if (wParam == (IntPtr)WM_MOUSEWHEEL)
                {
                    OnMouseWheel?.Invoke(this, new EventArgs());
                }
            }

            return CallNextHookEx(hHookID, nCode, wParam, lParam);
        }
    }

    public class MouseArgs : EventArgs
    {
        public MouseButton MouseButton { get; private set; }

        public MouseArgs(MouseButton mouseButton)
        {
            MouseButton = mouseButton;
        }
    }
}
