using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
/*
 * Maxwell.z(张向阳）made this origin project for vesystem
 */
namespace VEWebVideoService
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        /// 
        static bool isFirstRun()
        {
            RegistryKey reg = null;
            try
            {
                reg = Registry.LocalMachine.OpenSubKey("SOFTWARE\\VEWVRT", true);
            }
            catch {}

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
                reg = Registry.LocalMachine.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run", true);
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
        static Mutex g_Mutex = new Mutex(true, "{6C8A3355-6CB8-41A6-AF7F-5BBD8025228E}", out bEnable);
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

            HttpServer httpServer = new HttpServer();

            httpServer.Start("http://localhost:8233/");
            httpServer.Join();
            httpServer.Stop();
        }
    }
}
