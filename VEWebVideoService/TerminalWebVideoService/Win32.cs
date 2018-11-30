using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace TerminalWebVideoService
{
    class Win32
    {
    
        public enum ShowCommands : int
        {
            SW_HIDE = 0,
            SW_SHOWNORMAL = 1,
            SW_NORMAL = 1,
            SW_SHOWMINIMIZED = 2,
            SW_SHOWMAXIMIZED = 3,
            SW_MAXIMIZE = 3,
            SW_SHOWNOACTIVATE = 4,
            SW_SHOW = 5,
            SW_MINIMIZE = 6,
            SW_SHOWMINNOACTIVE = 7,
            SW_SHOWNA = 8,
            SW_RESTORE = 9,
            SW_SHOWDEFAULT = 10,
            SW_FORCEMINIMIZE = 11,
            SW_MAX = 11
        }
        [DllImport("shell32.dll")]
        static public extern IntPtr ShellExecute(
            IntPtr hwnd,
            string lpOperation,
            string lpFile,
            string lpParameters,
            string lpDirectory,
            ShowCommands nShowCmd);

        [System.Runtime.InteropServices.StructLayout(LayoutKind.Sequential)]
        public class SECURITY_ATTRIBUTES
        {
            public int nLength;
            public string lpSecurityDescriptor;
            public bool bInheritHandle;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct STARTUPINFO
        {
            public int cb;
            public string lpReserved;
            public string lpDesktop;
            public int lpTitle;
            public int dwX;
            public int dwY;
            public int dwXSize;
            public int dwYSize;
            public int dwXCountChars;
            public int dwYCountChars;
            public int dwFillAttribute;
            public int dwFlags;
            public int wShowWindow;
            public int cbReserved2;
            public byte lpReserved2;
            public IntPtr hStdInput;
            public IntPtr hStdOutput;
            public IntPtr hStdError;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct PROCESS_INFORMATION
        {
            public IntPtr hProcess;
            public IntPtr hThread;
            public int dwProcessId;
            public int dwThreadId;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MSG
        {
            public IntPtr hwnd;
            public int message;
            public int wParam;
            public int lParam;
            public int time;
            public int ptx;
            public int pty;
            public int lPrivate;
        }
      


        [DllImport("Kernel32.dll", CharSet = CharSet.Ansi)]
        static public extern int LoadModule(string lpModuleName,IntPtr lpParameterBlock);
        [DllImport("Kernel32.dll", CharSet = CharSet.Ansi)]
        static public extern IntPtr GetModuleHandle( string strModule);
        [DllImport("Kernel32.dll", CharSet = CharSet.Ansi)]
        static public extern bool CreateProcess(
            StringBuilder lpApplicationName, StringBuilder lpCommandLine,
            SECURITY_ATTRIBUTES lpProcessAttributes,
            SECURITY_ATTRIBUTES lpThreadAttributes,
            bool bInheritHandles,
            int dwCreationFlags,
            StringBuilder lpEnvironment,
            StringBuilder lpCurrentDirectory,
            ref STARTUPINFO lpStartupInfo,
            ref PROCESS_INFORMATION lpProcessInformation
            );

        public delegate bool EnumWindowsProc(IntPtr hWnd, int lParam);
        [DllImport("user32", EntryPoint = "EnumThreadWindows")]
        static public extern int EnumThreadWindows(int threadid, EnumWindowsProc ewp, int lParam);
        [DllImport("user32", EntryPoint = "EnumWindows")]
        static public extern int EnumWindows(EnumWindowsProc ewp, int lParam);
        [DllImport("user32.dll")]
        static public extern IntPtr GetDesktopWindow();
        [DllImport("user32.dll")]
        static public extern IntPtr GetParent(IntPtr hWnd);
        [DllImport("user32.dll")]
        static public extern bool ShowWindow(IntPtr hWnd, short State);
        [DllImport("user32.dll")]
        static public extern bool IsWindow(IntPtr hWnd);
        [DllImport("user32.dll")]
        static public extern IntPtr GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        static public extern IntPtr SetWindowLong(IntPtr hWnd, int nIndex, int val);
        [DllImport("user32.dll")]
        static public extern bool BringWindowToTop(IntPtr hWnd);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static public extern int GetWindowThreadProcessId(IntPtr handle, out int processId);
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static public extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);
        [DllImport("kernel32.dll", EntryPoint = "TerminateProcess", SetLastError = true, CharSet = CharSet.Auto)]
        static public extern int TerminateProcess(IntPtr h, uint time);
        [DllImport("kernel32.dll", SetLastError = true)]
        static public extern UInt32 WaitForSingleObject(IntPtr hHandle, UInt32 dwMilliseconds);
        [DllImport("user32.dll", EntryPoint = "FindWindowEx", SetLastError = true)]
        static public extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);
        [DllImport("user32.dll", EntryPoint = "SetParent", SetLastError = true)]
        static public extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);
        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        static public extern IntPtr GetForegroundWindow();
        [DllImport("User32.dll", EntryPoint = "SendMessage")]
        static public extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImport("User32.dll", EntryPoint = "PostMessage")]
        static public extern int PostMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        [DllImport("coredll.dll")]
        static public extern IntPtr LoadIcon(IntPtr hInst, IntPtr pName);
        [DllImport("user32.dll", EntryPoint = "SetWindowPos")]
        static public extern int SetWindowPos(int hwnd, int hWndInsertAfter, int x, int y, int cx, int cy, int wFlags);

        public struct Rect
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
             
        }

        [DllImport("User32.dll")]
        public static extern int GetWindowRect(IntPtr hwnd, out Rect lpRect);

        [DllImport("user32.dll")]
        public static extern int GetClientRect(IntPtr hwnd, out Rect lpRect);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int MoveWindow(IntPtr hWnd, int x, int y, int nWidth, int nHeight, bool BRePaint);

        public delegate int HookProc(int nCode, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll")]
        public static extern HookProc SetWindowsHookEx(int code, Win32.HookProc func, IntPtr hInstance, int threadID);


        [DllImport("user32.dll")]
        public static extern int SetWindowsHook(int idHook, Win32.HookProc lpfn, IntPtr hInstance, int threadId);

        [DllImport("user32.dll")]
        public static extern  int DefWindowProcA(IntPtr hWnd, int Msg, IntPtr wparam, IntPtr lParam);
        [DllImport("Kernel32.dll")]
        public static extern IntPtr OpenProcess(int dwDesiredAccess, int bInheritHandle, int dwProcessId);
        [DllImport("user32.dll")]
        public static extern  int CallNextHookEx(HookProc hhk,int nCode, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll")]
        public static extern int CallNextHook(HookProc hhk, int nCode, IntPtr wParam, IntPtr lParam);
        [DllImport("Kernel32.dll")]
        public static extern int GetLastError();
        [DllImport("user32.dll")]
        public static extern int CallWindowProc(HookProc hhk, IntPtr hwnd,int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll")]
        public static extern int GetCurrentThreadId();

    }
}
