using System;
using System.Collections.Generic;
using System.Xml;
using System.Management;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using Microsoft.Win32;

namespace VEDnsControl
{
     
    class VerifyCode
    {
        public static uint ReadFileRecVerifyCode(string regKey)
        {
            //host file 
            Microsoft.Win32.RegistryKey reg = null;
            try
            {
                reg = Registry.LocalMachine.OpenSubKey("SOFTWARE\\VEWVRT", true);
            }
            catch { }

            if (null == reg)
            {
                return 0;
            }

            try
            {
                return Convert.ToUInt32((string)reg.GetValue(regKey), 10); //"hostsverify"
            }
            catch { }
            return 0;
        }

        public static uint MakeFileVerifyCode(string strFile)
        {
            try
            {
                StreamReader bf = new StreamReader(File.Open(strFile, FileMode.Open, FileAccess.Read));
                string strContent = bf.ReadToEnd();
                uint verifyCode = 0;
                foreach (char ch in strContent)
                {
                    verifyCode += (uint)ch;
                }
                bf.Close();

                return verifyCode;
            }
            catch (Exception e)
            {
                // Log.Logger.Instance.WriteException(e);
                //uiListener.WriteOut(e.Message);
            }
            return 0;
        }

        public static void WriteFileVerifyCode(uint verifyCode ,string strKey)
        {
            //host file 
            Microsoft.Win32.RegistryKey reg = null;
            try
            {
                reg = Registry.LocalMachine.OpenSubKey("SOFTWARE\\VEWVRT", true);
            }
            catch { }

            if (null == reg)
            {
                reg = Registry.LocalMachine.CreateSubKey("SOFTWARE\\VEWVRT");
            }

            try
            {
                //return Convert.ToUInt32((string)reg.GetValue("dnscrack"), 10); ;
                string strValue = Convert.ToString(verifyCode, 10);
                reg.SetValue(strKey, strValue);
            }
            catch { }

        }

    }

    class DnsConfigLoader
    {
        public UIListener uiListener;
        public class DnsItem
        {
            public string strDomain;
            public string strIp;
            public DnsItem(string strDomain, string strIp)
            {
                this.strDomain = strDomain;
                this.strIp = strIp;
            }
        };

        public List<DnsItem> mDns = new List<DnsItem>();
        public string strFileName = AppDomain.CurrentDomain.BaseDirectory + "Dns.config";

        internal void Add(string item, string ip)
        {
            DnsItem it = new DnsItem(item, ip);
            mDns.Add(it);
        }

        internal void Remove(string item)
        {
            foreach( DnsItem it in mDns)
            {
                if (it.strDomain == item)
                {
                    mDns.Remove(it);
                    break;
                }
            }
        }

        public DnsConfigLoader(UIListener uilistener)
        {
            uiListener = uilistener;

            Check();
            Load();
        }

        public List<DnsItem> GetDnsList()
        {
            return mDns;
        }

        public bool Save()
        {
            string conf_hearder = "<?xml version=\"1.0\" encoding=\"utf-8\" ?>\n<configuration>\n<dnsList>";
            string conf_tail = "</dnsList>\n</configuration>";

            if (mDns.Count <= 0)
            {
                uiListener.WriteOut("没有发现可写的dns！");
                return false;
            }

            try
            {
                uiListener.WriteOut("开始写Dns.config文件..");
                StreamWriter sw = new StreamWriter(new FileStream(strFileName, FileMode.CreateNew));
                sw.WriteLine(conf_hearder);
                foreach (DnsItem it in mDns)
                {
                    sw.WriteLine("<add domain=\"" + it.strDomain + "\" ip=\"" + it.strIp + "\"/>");
                }

                sw.WriteLine(conf_tail);
                sw.Flush();
                sw.Close();
                uiListener.WriteOut("文件写入完毕！");
            }
            catch (IOException e)
            {
                uiListener.WriteOut(e.Message);
                File.Delete(strFileName);
                return false;
            }
            return true;
        }

        public bool SafeSave()
        {
            //bkup old
            string tempPath = Path.GetTempFileName();
            File.Delete(tempPath);
            File.Copy(strFileName, tempPath);
            //
            File.Delete(strFileName);

            string conf_hearder = "<?xml version=\"1.0\" encoding=\"utf-8\" ?>\n<configuration>\n<dnsList>";
            string conf_tail = "</dnsList>\n</configuration>";

            if (mDns.Count <= 0)
            {
                uiListener.WriteOut("没有发现可写的dns！");
                return false;
            }

            try
            {
                uiListener.WriteOut("开始写Dns.config文件..");
                StreamWriter sw = new StreamWriter(new FileStream(strFileName, FileMode.CreateNew));
                sw.WriteLine(conf_hearder);
                foreach (DnsItem it in mDns)
                {
                    sw.WriteLine("<add domain=\"" + it.strDomain + "\" ip=\"" + it.strIp + "\"/>");
                }

                sw.WriteLine(conf_tail);
                sw.Flush();
                sw.Close();
                uiListener.WriteOut("文件写入完毕！");
            }
            catch (IOException e)
            {
                uiListener.WriteOut(e.Message);
                File.Delete(strFileName);
                File.Copy(tempPath, strFileName);
                File.Delete(tempPath);
                return false;
            }
            File.Delete(tempPath);
            return true;
        }

        public bool SaveFile()
        {
            if (File.Exists(strFileName))
                return SafeSave();
            else
                return Save();
        }

        protected void Check()
        {
            uint verifyCode = VerifyCode.ReadFileRecVerifyCode("dnsconfverifycode");
            uint curVerifyCode = VerifyCode.MakeFileVerifyCode(strFileName);
            //重新设置hosts校验
            if (verifyCode == 0 || verifyCode != curVerifyCode)
                VerifyCode.WriteFileVerifyCode(0, "hostsverifycode");
        }

        protected bool Load()
        {
            //Load会重复调用，先清理一下旧数据
            mDns.Clear();
            try
            {
                XmlDocument doc = new XmlDocument();
                //获得配置文件的全路径

                doc.Load(this.strFileName);
                //找出名称为“add”的所有元素
                XmlNodeList dnsListSection = doc.GetElementsByTagName("dnsList");

                if (null != dnsListSection)
                {
                    for (int i = 0; i < dnsListSection.Count; i++)
                    {
                        XmlNode node = dnsListSection[i];
                        XmlNodeList nodes = node.SelectNodes("add");
                        foreach (XmlElement el in nodes)
                        {
                            DnsItem dnsItem = new DnsItem((string)el.Attributes["domain"].Value, (string)el.Attributes["ip"].Value);
                            mDns.Add(dnsItem);
                        }
                    }
                }

                doc = null;
            }
            catch (Exception e)
            {
                //Log.Logger.Instance.WriteException(e);
                uiListener.WriteOut(e.Message);
            }

            return mDns.Count > 0;
        }

        ~DnsConfigLoader()
        {
            mDns.Clear();
            mDns = null;
        }
    }

    //修改系统dns转向
    class DnsController
    {
        string strSysHosts = System.Environment.SystemDirectory + "\\drivers\\etc\\hosts";    //C:\Windows\System32\drivers\etc
        DnsConfigLoader dnsloader;
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern SafeFileHandle CreateFileA(string lpFileName, uint dwDesiredAccess, uint dwShareMode, IntPtr lpSecurityAttributes, uint dwCreationDisposition, uint dwFlagsAndAttributes, IntPtr hTemplateFile);
       
        public DnsController(DnsConfigLoader dnsloader)
        {
            this.dnsloader = dnsloader;
        }

        public bool HostsFileIsOk()
        {
            uint recVerifyCode = VerifyCode.ReadFileRecVerifyCode("hostsverifycode");
            uint curVerifyCode = VerifyCode.MakeFileVerifyCode(strSysHosts);
            if (recVerifyCode == 0 || curVerifyCode == 0)
                return false;
            return recVerifyCode == curVerifyCode;
        }

        public bool LoadDnsToSystem()
        {
           // if (false == DnsConfigLoader.Instance.Load())
           //     return false;
            if(!HostsFileIsOk())
                return UpdateDnsSystem(dnsloader);
            return true;
        }

        protected bool UpdateDnsSystem(DnsConfigLoader dnsInfo)
        {
            List<DnsConfigLoader.DnsItem> LDns = dnsInfo.GetDnsList();
            {
                //Log.Logger.Instance.WriteLog("no dns driver object:MicrosoftDNS !");
                UpdateRecordsToCacheFile(LDns);
                uint verifyCode = VerifyCode.MakeFileVerifyCode(strSysHosts);
                if (verifyCode == 0)
                    dnsloader.uiListener.WriteOut("Make host file verycode==0!");
                else
                    VerifyCode.WriteFileVerifyCode(verifyCode, "hostsverifycode");
            }
            
            return true;
        }

        protected void UpdateRecordsToCacheFile(List<DnsConfigLoader.DnsItem> dnsList)
        {
            try
            {
                FileStream fs = File.Open(strSysHosts, FileMode.Create, FileAccess.ReadWrite,FileShare.Read);
                if(fs != null)
                {
                    StreamWriter sw = new StreamWriter(fs);
                    foreach(DnsConfigLoader.DnsItem it in dnsList)
                    {
                        string strItem =  it.strIp +"     "  + it.strDomain;
                        sw.WriteLine(strItem);
                    }
                    sw.Close();
                    fs.Close();
                }
            }catch(Exception e)
            {
                dnsloader.uiListener.WriteOut(e.Message);
            }
        }

        internal void Applicate()
        {
            //保存
            this.dnsloader.SaveFile();
            this.dnsloader.uiListener.WriteOut("写出修改结果到文件..");
            //verify code
            uint verifycode = VerifyCode.MakeFileVerifyCode(dnsloader.strFileName);
            VerifyCode.WriteFileVerifyCode(verifycode, "dnsconfverifycode");
            this.dnsloader.uiListener.WriteOut("更新Config文件验证码..");
            //update to hosts
            UpdateRecordsToCacheFile(dnsloader.mDns);
            this.dnsloader.uiListener.WriteOut("更新hosts文件..");
            //verify code
            verifycode = VerifyCode.MakeFileVerifyCode(this.strSysHosts);
            this.dnsloader.uiListener.WriteOut("更新hosts文件验证码..");
            VerifyCode.WriteFileVerifyCode(verifycode, "hostsverifycode");
            this.dnsloader.uiListener.WriteOut("应用完成");
        }
    }
    
}
