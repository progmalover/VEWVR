using System;
using System.Collections.Generic;
using System.Xml;
using System.Management;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using Microsoft.Win32;

namespace VEWebUrlRedirectService
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
                reg.SetValue(strKey, Convert.ToString(verifyCode, 10));
            }
            catch { }

        }

    }

    class DnsConfigLoader
    {
       public class DnsItem
        {
            public string strDomain;
            public string strIp;
            public DnsItem(string strDomain,string strIp)
            {
                this.strDomain = strDomain;
                this.strIp = strIp;
            }
        };
        List<DnsItem> mDns = new List<DnsItem>();
        string strFileName = AppDomain.CurrentDomain.BaseDirectory + "Dns.config";
        public DnsConfigLoader()
        {
            Check();
            Load();
        }

        public List<DnsItem> GetDnsList()
        {
            return mDns;
        }

        protected void Check()
        {
            uint verifyCode = VerifyCode.ReadFileRecVerifyCode("dnsconfverifycode");
            uint curVerifyCode = VerifyCode.MakeFileVerifyCode(strFileName);
            //重新设置hosts校验
            if (verifyCode == 0 || verifyCode != curVerifyCode)
            {
                VerifyCode.WriteFileVerifyCode(0, "hostsverifycode");
                VerifyCode.WriteFileVerifyCode(curVerifyCode, "dnsconfverifycode");
            }

        }

        protected bool Load()
        {
            //Load会重复调用，先清理一下旧数据
            mDns.Clear();
            try
              {
                XmlDocument doc = new XmlDocument();
                //获得配置文件的全路径
               
                doc.Load(strFileName);
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
            }
            catch (Exception e)
            {
                Log.Logger.Instance.WriteException(e);
            }

            return mDns.Count > 0;
        }

        ~DnsConfigLoader()
        {
            mDns.Clear();
            mDns = null;
        }

        public static DnsConfigLoader Instance = new DnsConfigLoader();
    }

    //修改系统dns转向
    class DnsController
    {
         string strSysHosts = System.Environment.SystemDirectory + "\\drivers\\etc\\hosts";    //C:\Windows\System32\drivers\etc
            
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern SafeFileHandle CreateFileA(string lpFileName, uint dwDesiredAccess, uint dwShareMode, IntPtr lpSecurityAttributes, uint dwCreationDisposition, uint dwFlagsAndAttributes, IntPtr hTemplateFile);
       
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
                return UpdateDnsSystem(DnsConfigLoader.Instance);
            return true;
        }

        protected bool UpdateDnsSystem(DnsConfigLoader dnsInfo)
        {
            List<DnsConfigLoader.DnsItem> LDns = dnsInfo.GetDnsList();
            bool bDnsInst = CheckDnsInst();
            if (!bDnsInst)
            {
                //Log.Logger.Instance.WriteLog("no dns driver object:MicrosoftDNS !");
                UpdateRecordsToCacheFile(LDns);
                uint verifyCode = VerifyCode.MakeFileVerifyCode(strSysHosts);
                if (verifyCode == 0)
                    Log.Logger.Instance.WriteLog("Make host file verycode==0!");
                else
                    VerifyCode.WriteFileVerifyCode(verifyCode, "hostsverifycode");
            }
            else
            foreach (DnsConfigLoader.DnsItem item in LDns)
            {
                UpdateARecord("", item.strDomain, item.strIp);
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
               // Log.Logger.Instance.WriteException(e);
            }
        }

        private bool CheckDnsInst()
        {
            try
            {
                SafeFileHandle sfh = CreateFileA(@"\\.\ROOT\MicrosoftDNS", 0x80000000, 0x00000001,IntPtr.Zero,4,0x80| 0x40000000,IntPtr.Zero);
                if (sfh.IsInvalid)
                    return false;
                sfh.Close();
            }
            catch(Exception e) {
                //Log.Logger.Instance.WriteException(e);
                return false;
            }

            return true;
        }

        private bool UpdateARecord(string strDNSZone, string strHostName, string strIPAddress)
        {
            ManagementScope mgmtScope = new ManagementScope(@"\\.\ROOT\MicrosoftDNS");
            ManagementClass mgmtClass = null;
            ManagementBaseObject mgmtParams = null;
            ManagementObjectSearcher mgmtSearch = null;
            ManagementObjectCollection mgmtDNSRecords = null;
            string strQuery;

            strQuery = string.Format("SELECT * FROM MicrosoftDNS_AType WHERE OwnerName = '{0}.{1}'", strHostName, strDNSZone);

            try
            {
                mgmtSearch = new ManagementObjectSearcher(mgmtScope, new ObjectQuery(strQuery));
                mgmtDNSRecords = mgmtSearch.Get();
            }
            catch {
                return false;
            }

            // Multiple A records with the same record name, but different IPv4 addresses, skip.    
            if (mgmtDNSRecords.Count > 1)
            {
                // Take appropriate action here.     
            }
            // Existing A record found, update record.    
            else if (mgmtDNSRecords.Count == 1)
            {
                foreach (ManagementObject mgmtDNSRecord in mgmtDNSRecords)
                {
                    if (mgmtDNSRecord["RecordData"].ToString() != strIPAddress)
                    {
                        mgmtParams = mgmtDNSRecord.GetMethodParameters("Modify");
                        mgmtParams["IPAddress"] = strIPAddress;

                        mgmtDNSRecord.InvokeMethod("Modify", mgmtParams, null);
                    }

                    break;
                }
            }
            // A record does not exist, create new record.    
            else
            {
                mgmtClass = new ManagementClass(mgmtScope, new ManagementPath("MicrosoftDNS_AType"), null);

                mgmtParams = mgmtClass.GetMethodParameters("CreateInstanceFromPropertyData");
                mgmtParams["DnsServerName"] = Environment.MachineName;
                mgmtParams["ContainerName"] = strDNSZone;
                mgmtParams["OwnerName"] = string.Format("{0}.{1}", strHostName.ToLower(), strDNSZone);
                mgmtParams["IPAddress"] = strIPAddress;

                mgmtClass.InvokeMethod("CreateInstanceFromPropertyData", mgmtParams, null);
            }
            return true;
        }
    }
    
}
