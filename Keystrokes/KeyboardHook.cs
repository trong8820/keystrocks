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
    class KeyboardHook
    {
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x100;
        private const int WM_KEYUP = 0x101;
        private const int WM_SYSKEYDOWN = 0x104;
        private const int WM_SYSKEYUP = 0x105;

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, KeyboardHookProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        public delegate IntPtr KeyboardHookProc(int nCode, IntPtr wParam, IntPtr lParam);

        public event EventHandler<KeyArgs> OnKeyPressed;
        public event EventHandler<KeyArgs> OnKeyUp;

        private readonly KeyboardHookProc keyboardHookProc;
        private IntPtr hHookID = IntPtr.Zero;

        public KeyboardHook()
        {
            keyboardHookProc = HookCallback;
        }

        public void HookKeyboard()
        {
            hHookID = SetHook(keyboardHookProc);
        }

        public void UnHookKeyboard()
        {
            UnhookWindowsHookEx(hHookID);
        }

        private IntPtr SetHook(KeyboardHookProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                if (wParam == (IntPtr)WM_KEYDOWN || wParam == (IntPtr)WM_SYSKEYDOWN)
                {
                    int vkCode = Marshal.ReadInt32(lParam);

                    OnKeyPressed?.Invoke(this, new KeyArgs(KeyInterop.KeyFromVirtualKey(vkCode)));
                }

                if (wParam == (IntPtr)WM_KEYUP || wParam == (IntPtr)WM_SYSKEYUP)
                {
                    int vkCode = Marshal.ReadInt32(lParam);

                    OnKeyUp?.Invoke(this, new KeyArgs(KeyInterop.KeyFromVirtualKey(vkCode)));
                }
            }

            return CallNextHookEx(hHookID, nCode, wParam, lParam);
        }
    }

    public class KeyArgs : EventArgs
    {
        public Key Key { get; private set; }

        public KeyArgs(Key key)
        {
            Key = key;
        }
    }
}
