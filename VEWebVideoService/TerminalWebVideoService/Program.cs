using System;
using System.Threading;
using Microsoft.Win32;
/*
 * Maxwell.z(张向阳）made this origin project for vesystem
 */
namespace TerminalWebVideoService
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
                reg = Registry.LocalMachine.OpenSubKey("SOFTWARE\\VEWVRT",true);
            }
            catch {}

            string runKey = "";
            if (null == reg)
            {
                reg = Registry.LocalMachine.CreateSubKey("SOFTWARE\\VEWVRT");
                reg.SetValue("firstrun_t","0");
                return true;
            }
            
            runKey = (string)reg.GetValue("firstrun_t");
            if (runKey == "0")
                return false;

            reg.SetValue("firstrun_t", "0");
            return true;
        }

        static void RegAutoStart()
        {
            RegistryKey reg = null;
            try
            {
                reg = Registry.LocalMachine.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run",true);
                if (null != reg)
                {
                    string strExePath = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
                    strExePath += "  -s";
                    reg.SetValue("VEWVRTService", strExePath);
                }
            }
            catch(Exception e){
                Log.Logger.Instance.WriteException(e);
            }
        }

        static bool  bEnable;
        static Mutex g_Mutex  = new Mutex(true, "{F55FEA51-0D8F-4949-99EF-2C6EB332930A}", out bEnable);
        static void Main()
        {
            if (bEnable == false)
            {
                Log.Logger.Instance.WriteLog("服务实例已经存在！");
                return;
            }
          
			try{
				if(isFirstRun())
				{
					RegAutoStart();
				}
			}catch(Exception e)
			{  
				Log.Logger.Instance.WriteException(e);
			}

#if DEBUG
            while(true)
            {
                InvokeBrowser.CallWebBrowser("https://www.iqiyi.com");
                Thread.Sleep(5000);
                InvokeBrowser.CallWebBrowser("https://www.youku.com");
                Thread.Sleep(5000);
            }
#else
            Service service = new Service();
            service.Start();
            service.Join();
#endif

        }
    }
}


