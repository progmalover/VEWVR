using Microsoft.Win32;
using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
/*
* Notice
* This code is designed for vesystem,all right reserved. --maxwell.z
* 2018.12
*/
namespace VEWebUrlRedirectService
{
    class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        /// 
        static bool isFirstRun()
        {
            Microsoft.Win32.RegistryKey reg = null;
            try
            {
                reg = Registry.LocalMachine.OpenSubKey("SOFTWARE\\VEWVRT", true);
            }
            catch { }

            string runKey = "";
            if (null == reg)
            {
                reg = Registry.LocalMachine.CreateSubKey("SOFTWARE\\VEWVRT");
                reg.SetValue("firstrun_r", "0");
                return true;
            }

            runKey = (string)reg.GetValue("firstrun_r");
            if (runKey == "0")
                return false;

            reg.SetValue("firstrun_r", "0");
            return true;
        }

        static void RegAutoStart()
        {
            RegistryKey reg = null;
            try
            {
                reg = Microsoft.Win32.Registry.LocalMachine.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                if (null != reg)
                {
                    string strExePath = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
                    strExePath += "  -s";
                    reg.SetValue("VEWVRVService", strExePath);
                }
            }
            catch (Exception e)
            {
                Log.Logger.Instance.WriteException(e);
            }
        }

        static bool bEnable;
        static Mutex g_Mutex = new Mutex(true, "{6ACC3355-6CA4-3319-AF7F-5BABCD25228E}", out bEnable);
        static void Main()
        {
            if (bEnable == false)
            {
                Log.Logger.Instance.WriteLog("服务实例已经存在！");
                return;
            }

            try
            {
                if (isFirstRun())
                {
                    RegAutoStart();
                }
            }
            catch (Exception e)
            {
                Log.Logger.Instance.WriteException(e);
            }

            //修改window dns服务,转向网站访问
            DnsController dCtrl = new DnsController();
            dCtrl.LoadDnsToSystem();

    
            //url数据发送器，发送数据到终端
            TcpDataSender.GetInst().Start();

            HttpServer httpServer = new HttpServer();
            httpServer.Start("http://127.0.0.1:80/");

            HttpWebSocketServer webSokectServer = new HttpWebSocketServer();
            webSokectServer.Start("http://127.0.0.1:8233/");

            HttpsServer httpsServer = new HttpsServer(443);
            httpsServer.Start();
            ExitSignal.Join();
        }
    }
}
