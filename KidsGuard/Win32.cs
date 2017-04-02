using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace KidsComputerGuard
{
    class Win32
    {
        // used for an output LPCTSTR parameter on a method call
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct STRINGBUFFER
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public string szText;
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr FindWindowEx(IntPtr parent /*HWND*/,
                                                 IntPtr next /*HWND*/,
                                                 string sClassName,
                                                 IntPtr sWindowTitle);

        [DllImport("user32.dll")]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        public const int WM_SETTEXT = 0X000C;
        public const int WM_GETTEXT = 0X000D;
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int SendMessage(IntPtr hWnd,
            int msg, int wParam, out STRINGBUFFER ClassName);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int SendMessage(IntPtr hWnd, int msg, int wParam, StringBuilder lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int GetWindowText(IntPtr hWnd, out STRINGBUFFER ClassName, int nMaxCount);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int GetClassName(IntPtr hWnd, out STRINGBUFFER ClassName, int nMaxCount);

        //[DllImport("user32.dll")]
        //[return: MarshalAs(UnmanagedType.Bool)]
        //static extern bool EnumChildWindows(IntPtr hwndParent, EnumWindowsProc lpEnumFunc, IntPtr lParam);

        public delegate bool Win32Callback(IntPtr hwnd, IntPtr lParam);

        [DllImport("user32.Dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool EnumChildWindows(IntPtr parentHandle, Win32Callback callback, IntPtr lParam);

        /// <summary>
        /// Helper to get window classname
        /// </summary>
        /// <param name="hWnd"></param>
        /// <returns></returns>
        static public string GetClassName(IntPtr hWnd)
        {
            Win32.STRINGBUFFER sLimitedLengthWindowTitle;
            Win32.GetClassName(hWnd, out sLimitedLengthWindowTitle, 256);
            return sLimitedLengthWindowTitle.szText;
        }

        /// <summary>
        /// Helper to get window text
        /// </summary>
        /// <param name="hWnd"></param>
        /// <returns></returns>
        static public string GetWindowText(IntPtr hWnd)
        {
            Win32.STRINGBUFFER sLimitedLengthWindowTitle;
            SendMessage(hWnd, WM_GETTEXT, 256, out sLimitedLengthWindowTitle);
            //Win32.GetWindowText(hWnd, out sLimitedLengthWindowTitle, 256);
            return sLimitedLengthWindowTitle.szText;
        }

        static public string GetActiveWindowTitle()
        {
            IntPtr handle = GetForegroundWindow();

            const int nChars = 256;
            StringBuilder Buff = new StringBuilder(nChars);
            if (GetWindowText(handle, Buff, nChars) > 0)
            {
                return Buff.ToString();
            }
            return null;
        }

        static public string GetActiveProcessName()
        {
            IntPtr handle = GetForegroundWindow();

            int pid = 0;
            GetWindowThreadProcessId(handle, out pid);

            if (pid == 0)
                return null;

            return Process.GetProcessById(pid).ProcessName;
        }

        static public void KillActiveProcess()
        {
            IntPtr handle = GetForegroundWindow();

            int pid = 0;
            GetWindowThreadProcessId(handle, out pid);

            if (pid != 0)
            {
                Process.GetProcessById(pid).Kill();
            }
        }

        [DllImport("user32.dll", SetLastError = true)]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out int processId);

        [DllImport("user32")]
        public static extern void LockWorkStation();
    }
}
