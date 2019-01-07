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

        //this code maybe is cause of clr.dll crash, i tried best to fix it,but failed..so
        //don't call this code till i know how to fix issue.
        //but it's always ok in my development pc,even run thousands of times....
        /*
         *    故障模块名称:	clr.dll
              故障模块版本:	4.7.2650.0
              故障模块时间戳:	5ab1c520
              异常代码:	c0000005
              异常偏移:	00000000001c50b9
              OS 版本:	6.1.7601.2.1.0.256.1
         */
         //以上问题已经修正，去掉不安全的win32 api调用。
        //static object g_calllocker = new object();
        public static bool CreateFloatControlBar(string exePath,IntPtr hProcess, IntPtr hOwner ,IntPtr hBrowserWnd)
        {
            new Thread(() => {
                //lock (g_calllocker)
                {
                    using (FloatCtrlBar _bar = new FloatCtrlBar(hOwner, hBrowserWnd, hProcess, exePath))
                    {
                        try { Application.Run((Form)_bar); }
                        catch { }
                    }
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
                    Win32.ShowWindow(g_CurBrowserWnd, 3);
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
                        try
                        {
                            item.Process.Kill();
                        }
                        catch { }
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
            sInfo.wShowWindow = 0;
            Win32.PROCESS_INFORMATION pInfo = new Win32.PROCESS_INFORMATION();
            bool bSucc = Win32.CreateProcess(null,
                new StringBuilder(strBrowserPath + " " + strUrl), null, null, true, 0, null, null, ref sInfo, ref pInfo);

            if (!bSucc)
                throw new Exception("create process failed!");

            //进程实例运行中
            uint ret = Win32.WaitForSingleObject(pInfo.hProcess, 1);
            if (ret != 0)
            {
                //wait  3 seconds for the webbrowser window complete.
                //Thread.Sleep(2000);
                hProcess = pInfo.hProcess;
                //尝试重抓5次
                int tryTimes = 5;
                while (--tryTimes >= 0)
                {
                    hWndBrowser = GetThreadMainWindow(pInfo.dwThreadId);
                    if (hWndBrowser != IntPtr.Zero)
                        break;
                    Thread.Sleep(500);
                }
                //等待子窗口附加成功
                int waitCount = 0;
                while(true)
                {
                    if (!Win32.IsWindow(hWndBrowser))
                    {
                        return IntPtr.Zero;
                    }

                    //main window is ok
                    IntPtr hwnd = Win32.GetWindow(hWndBrowser, 5);
                    if(hwnd != IntPtr.Zero)
                    {
                        bool bIsWinRun = Win32.IsWindow(hwnd);
                        if (bIsWinRun)
                            break;
                        else
                            return IntPtr.Zero;
                    }
                    //防止测试中发现的“野窗口”
                    Thread.Sleep(200);
                    waitCount++;
                    if (waitCount >= 15)
                        return IntPtr.Zero;
                }

                return hWndBrowser;
            }
            else
            {
               Log.Logger.Instance.WriteLog("未能成功发现浏览器新的主窗口实例！");
            }
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

        /**
         * 浏览器主窗口主窗口已经重合成功
         */
        private static bool MainWebBrowserIsRead(IntPtr hWeb)
        {
            
            long style = Win32.GetWindowLong(hWeb, -16).ToInt64();
            if ((style & 0x00020000L) == 0x00020000L || (style & 0x00010000L) == 0x00010000L)
            {
                IntPtr hwnd = Win32.GetWindow(hWeb, 5);
                if (hwnd != IntPtr.Zero && Win32.IsWindow(hwnd))
                    return true;
            }
            return false;
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
