using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
 
using System.Diagnostics;
using System.Text;
using System.Runtime.InteropServices;

namespace TerminalWebVideoService
{
    
    class Service
    {
        #region Win32
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
        static extern IntPtr ShellExecute(
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

        [DllImport("Kernel32.dll", CharSet = CharSet.Ansi)]
        public static extern bool CreateProcess(
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
        #endregion

        private volatile bool  _runing;
        private Thread _mainThread;
        TcpListener _listener = null;
        public Service()
        {
            _runing = false;
            _mainThread = null;
        }
        public void Start()
        {
            if(null == _mainThread)
                _mainThread = new Thread(DoLoop);
            _mainThread.Start();
        }

        public void DoLoop()
        {
            _runing = true;

            //如果绑定失败，不能退出主程序，做持续绑定动作
            while (_runing)
            {
                try
                {
                    _listener = new TcpListener(IPAddress.Parse("127.0.0.1"), 1886);
                    _listener.Start();
                    break;
                } catch (Exception e)
                {
                    Log.Logger.Instance.WriteLog(e.Message);
                    Thread.Sleep(5000);
                }
            }

            //accepting loop
            while (_runing)
            {
                TcpClient client = null;
                try
                {
                    client = _listener.AcceptTcpClient();
                }
                catch {
                    Thread.Sleep(5000);
                    client = null;
                }

                if (client == null)
                    continue;

                new Thread((param) =>
                {
                    //reading loop
                    TcpClient _client = (TcpClient)param;
                    StreamReader sr = null; 
                    string line = null;
                    while (_runing)
                    {
                        try
                        {
                            if(sr == null)
                                sr = new StreamReader(_client.GetStream());

                            while ((line = sr.ReadLine()) != null)
                            {
                                Log.Logger.Instance.WriteLog(line);
                                if (line.Contains("http"))
                                {
                                    try
                                    {
                                        Process process = new Process();
                                        process.StartInfo.UseShellExecute = true;
                                        process.StartInfo.FileName = line;
                                        process.StartInfo.CreateNoWindow = false;
                                        process.StartInfo.Verb = "open";

                                        Log.Logger.Instance.WriteLog("开始调用");
                                        process.Start();
                                        //STARTUPINFO si = new STARTUPINFO();
                                        //PROCESS_INFORMATION pi = new PROCESS_INFORMATION();

                                        //si.dwFlags = 1;
                                        //si.wShowWindow = 1; //TRUE表示显示创建的进程的窗口
                                        //si.lpDesktop = "default";

                                        //CreateProcess(null, new StringBuilder("webbrowser"), null, null, false, 0x10, null, null, ref si, ref pi);


                                    }
                                    catch (Exception e)
                                    {
                                        Log.Logger.Instance.WriteLog(e.Message);
                                    }
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            Log.Logger.Instance.WriteLog(e.Message);
                            if (null != sr)
                                sr.Close();
                            _client.Close();
                            break;
                        }
                    }

                }).Start(client);

            }
        }

        public void Stop()
        {
            _runing = false;

            if (null != this._listener)
                _listener.Stop();
            _mainThread.Join();
            _mainThread = null;
        }
    }

}
