using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading;
using System.Windows.Forms;
 
namespace TerminalWebVideoService
{
    class InvokeBrowser
    {
        static string g_CurUrl = "";
        static IntPtr g_CurBrowserWnd = IntPtr.Zero;
        
        private static IntPtr CreateEmbededWindow(string strWebBrowserPath ,string strUrl)
        {
            IntPtr CurBrowserProcess = IntPtr.Zero;
            IntPtr hBrowserWnd = DoCall(strWebBrowserPath, strUrl,ref CurBrowserProcess);
            
            if (hBrowserWnd != IntPtr.Zero)
            {
                //VMUIFrame
                IntPtr topWnd = Win32.FindWindowEx(IntPtr.Zero, IntPtr.Zero, "gdkWindowToplevel", null);
                IntPtr viewWnd = Win32.FindWindowEx(topWnd, IntPtr.Zero, "gdkWindowChild", null);

                //IntPtr topWnd = Win32.FindWindowEx(IntPtr.Zero, IntPtr.Zero, "VMUIFrame", null);
                //IntPtr viewWnd = Win32.FindWindowEx(topWnd, IntPtr.Zero, "VMUIView", null);// 
                //viewWnd = Win32.FindWindowEx(viewWnd, IntPtr.Zero, "AtlAxWin90", null);
               
                if (viewWnd != IntPtr.Zero)
                {
                   Win32.SetParent(hBrowserWnd, viewWnd);
                   CreateFloatControlBar(strWebBrowserPath, CurBrowserProcess, viewWnd, hBrowserWnd);
                }

                Win32.ShowWindow(hBrowserWnd, 3);
                Win32.BringWindowToTop(hBrowserWnd);
            }
            return hBrowserWnd;
        }

        public static bool CreateFloatControlBar(string exePath,IntPtr hProcess, IntPtr hOwner ,IntPtr hBrowserWnd)
        {
            // _bar.Show();
            new Thread(() => {
                using (FloatCtrlBar _bar = new FloatCtrlBar(hOwner, hBrowserWnd, hProcess, exePath))
                {
                    Application.Run((Form)_bar);
                }
            }
            ).Start();
            
            return true;
        }

        public static bool CallWebBrowser(string strUrl)
        {
            //同样的url，检查浏览器状态
            if(strUrl.CompareTo(g_CurUrl) == 0 &&
                        g_CurBrowserWnd != IntPtr.Zero)
            {
                //如果是最小化状态
                if(Win32.IsWindow(g_CurBrowserWnd))
                {
                    Win32.ShowWindow(g_CurBrowserWnd, 2);
                    return true;
                }
                g_CurBrowserWnd = IntPtr.Zero;
            }

            //因为浏览器多进程实例同时运行无法正常获取窗口句柄，所以使用系统单一实例的方法。
            //新url到来，杀掉当前多实例
            string strWebBrowserPath = GetWebBrowserInstPath();
            KillProcessInstsByPath(strWebBrowserPath);
            g_CurBrowserWnd = CreateEmbededWindow(strWebBrowserPath, strUrl);

            if (g_CurBrowserWnd != IntPtr.Zero)
            {
                g_CurUrl = strUrl;
            }

            return g_CurBrowserWnd != IntPtr.Zero;
        }

        //wmi ，scan for web browser instances
        public static bool KillProcessInstsByPath(string path)
        {
            //scan all process kill same path exe.
            string wmiQueryString = "SELECT ProcessId, ExecutablePath, CommandLine FROM Win32_Process";
            using (var searcher = new ManagementObjectSearcher(wmiQueryString))
            using (var results = searcher.Get())
            {
                var query = from p in Process.GetProcesses()
                            join mo in results.Cast<ManagementObject>()
                            on p.Id equals (int)(uint)mo["ProcessId"]
                            select new
                            {
                                Process = p,
                                Path = (string)mo["ExecutablePath"],
                                CommandLine = (string)mo["CommandLine"],
                            };

                bool bKill = false;
                foreach (var item in query)
                {
                    if (item.Path != null && item.Path.CompareTo(path) == 0)
                    {
                        item.Process.Kill();
                        bKill = true;
                    }
                }
                //新杀进程，等待1秒，到进程完全退出
                if (bKill)
                    Thread.Sleep(1000);
            }
            return true;
        }

        public static string  GetWebBrowserInstPath()
        {
            string strSubkey = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\App Paths\\chrome.exe";
            string strBrowserPath = "";

            //优先启动chrome
            try
            {
                RegistryKey regkey = Registry.LocalMachine.OpenSubKey(strSubkey);
                if (null != regkey)
                {
                    strBrowserPath = (string)regkey.GetValue("");
                    //交验地址
                    if (!File.Exists(strBrowserPath))
                        strBrowserPath = "";
                }
            }
            catch { }

            //未发现chrome,查找当前注册浏览器
            if (strBrowserPath == "")
            {
                strSubkey = "Software\\Classes\\http\\shell\\open\\command"; 
                try
                {
                    RegistryKey regkey = Registry.CurrentUser.OpenSubKey(strSubkey);
                    if (null != regkey)
                    {
                        strBrowserPath = (string)regkey.GetValue("");
                        strBrowserPath = strBrowserPath.Replace('\"', ' ').Trim();
                        strBrowserPath = strBrowserPath.Substring(0, strBrowserPath.IndexOf(".exe") + 4);

                        //交验地址
                        if (!File.Exists(strBrowserPath))
                            strBrowserPath = "";
                    }
                }
                catch { }
            }
            return strBrowserPath;
        }

        public static  IntPtr DoCall(string strBrowserPath, string strUrl ,ref IntPtr hProcess)
        {
            //必须确定进程启动成功，并且主窗口可以抓到
            IntPtr hWndBrowser = IntPtr.Zero;
            hProcess = IntPtr.Zero;

            Win32.STARTUPINFO sInfo = new Win32.STARTUPINFO();
            sInfo.wShowWindow = 2;
            Win32.PROCESS_INFORMATION pInfo = new Win32.PROCESS_INFORMATION();
            bool bSucc = Win32.CreateProcess(null,
                new StringBuilder(strBrowserPath + " " + strUrl), null, null, true, 0, null, null, ref sInfo, ref pInfo);

            if (!bSucc)
                throw new Exception("create process failed!");

            //进程实例运行中
            uint ret = Win32.WaitForSingleObject(pInfo.hProcess, 1);
            if (ret != 0)
            {

                hProcess = pInfo.hProcess;
                //尝试重抓5次
                int tryTimes = 5;
                while( --tryTimes >=0)
                {
                    hWndBrowser = GetThreadMainWindow(pInfo.dwThreadId);
                    if (hWndBrowser != IntPtr.Zero)
                        break;
                    Thread.Sleep(200);
                }

                return hWndBrowser;
            }
            else
                throw new Exception("未能成功发现浏览器新的主窗口实例！");
            return IntPtr.Zero; 
        }

        private static IntPtr DeeplyScanMainWnd(IntPtr hwnd)
        {
            if(hwnd == IntPtr.Zero)
                return IntPtr.Zero;

            long style = Win32.GetWindowLong(hwnd, -16).ToInt64();
            if ((style & 0x00020000L) == 0x00020000L || (style & 0x00010000L) == 0x00010000L)
            {
                return hwnd;
            }

            IntPtr pWnd = Win32.GetParent(hwnd);
            if (pWnd != IntPtr.Zero)
               return DeeplyScanMainWnd(pWnd);
            return IntPtr.Zero;
        }
        /*
        * 获取浏览器实例窗口的主窗口
        */
        private static IntPtr GetThreadMainWindow(int trdId)
        {
            IntPtr mainWnd = IntPtr.Zero;
            Win32.EnumThreadWindows(trdId, new Win32.EnumWindowsProc((hwnd,param)=>{

                 IntPtr pWnd = DeeplyScanMainWnd(hwnd);
                 if (pWnd != IntPtr.Zero)
                 {
                     mainWnd = pWnd;
                     return false;
                 }
                 return true;
            }), trdId);

            return mainWnd;
        }
    }
}
