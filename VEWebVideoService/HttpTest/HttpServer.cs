using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Microsoft.Win32;
/*
 * 浏览器数据转发服务
 */
namespace VEWebVideoService
{
    public class TcpDataSender
    {
        protected TcpClient client;
        protected volatile bool bExit;
        protected Queue<object> mque;
        protected Thread mWorkThread;
        protected Object mLock;
        protected string strTermIp;
        protected int iTermPort;
        protected AutoResetEvent waitEvent;
        public TcpDataSender()
        {
            waitEvent = new AutoResetEvent(false);
            client = new TcpClient();
            client.SendTimeout = 5000;
            client.SendBufferSize = 4096;
            mque = new Queue<object>();
            mLock = new Object();
            strTermIp = "";
            iTermPort = 0;
        }

        //从注册表获取终端ip地址
        public void QueryIpAddress(ref string strIp, ref int port)
        {

#if DEBUG
            strIp = "127.0.0.1";
            port = 1886;
#else
            RegistryKey reg = null;
            try
            {
                reg = Registry.LocalMachine.OpenSubKey("SYSTEM\\CurrentControlSet\\services\\sdserver");
            }
            catch{}

            if(reg == null){
                throw new Exception("注册表键值不存在！");
            }

            strIp = (string)reg.GetValue("ClientIp");
            if (strIp == null){
                throw new Exception("无法获得终端ip!");
            }else
                Log.Logger.Instance.WriteLog("获得终端ip:" + strIp);
            port = 1886;
          
#endif
        }

        public void RenewClient()
        {
            string tmpIp = "";
            int tmpPort = 0;
            //未连接返回
            if (!client.Connected)
                return;

            try
            {
                QueryIpAddress(ref tmpIp, ref tmpPort);
                if (tmpIp.CompareTo(strTermIp) != 0 || tmpPort != iTermPort)
                {
                    iTermPort = tmpPort;
                    strTermIp = tmpIp;

                    try
                    {
                        client.Close();
                        client = new TcpClient();
                        client.Connect(strTermIp, iTermPort);
                    }
                    catch (Exception e)
                    {
                        Log.Logger.Instance.WriteLog(e.Message);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Logger.Instance.WriteLog(e.Message);
            }
        }

        //推送url到终端
        public void PushUrl(string strUrl)
        {
            try
            { 
                //未链接状态，重新获取ip
                if (!client.Connected)
                {
                    QueryIpAddress(ref strTermIp, ref iTermPort);
                    client.Connect(strTermIp, iTermPort);
                }
            }
            catch (Exception e)
            {
                //Console.WriteLine(e.Message);
                Log.Logger.Instance.WriteLog(e.Message);
                return;
            }

            lock(mLock)
            {
                mque.Enqueue(strUrl);
                waitEvent.Set();
            }

            //产生异常后，模块可以重入
            if (mWorkThread == null)
            {
                mWorkThread = new Thread(() =>
                {
                    bExit = false;
                    while (!bExit) {
                        
                        while (mque.Count <= 0)
                        {
                            //wait for message - 使用event机制
                            waitEvent.WaitOne();
                            if (bExit)
                                return;
                        }
                        //读写保护
                        lock (mLock)
                        {
                            BinaryWriter bw = new BinaryWriter(client.GetStream());
                            string str = (string)mque.Dequeue();
                            //Console.WriteLine(str);
                            byte[] bytes = Encoding.UTF8.GetBytes(str);
                            try
                            {
                                bw.Write(bytes, 0, bytes.Length);
                                bw.Flush();
                                bw = null;
                            }catch
                            {
                                //网络对象清理
                                bw.Close();
                                client.Close();
                                
                                //准备逻辑重入
                                client = new TcpClient();
                                //当前线程退出
                                bExit = true;
                                //detach 线程
                                mWorkThread = null;
                                break;
                            }
                        }
                    }
                });

                mWorkThread.Start();
            } 
        }

        public void Exit()
        {
            bExit = true;
            if (mWorkThread != null)
                mWorkThread.Join();
            mWorkThread = null;
        }
    }

    public class HttpServer
    {
        protected HttpListener  Listener;
        protected TcpDataSender DataSender;
        
        public HttpServer()
        {
            DataSender = new TcpDataSender();
            Listener =   new HttpListener();
        }

        //start httpserv
        public void Start(string strUrl)
        {
            try
            {
                if (Listener == null)
                    Listener = new HttpListener();
                if (!Listener.IsListening)
                {
                    Listener.Prefixes.Add(strUrl);
                    Listener.Start();
                    IAsyncResult result = Listener.BeginGetContext(
                    new AsyncCallback(WebRequestCallback), this.Listener);
                }
            }catch(Exception e)
            {
                Log.Logger.Instance.WriteException(e);
            }
        }

        //stop httpserv
        public void Stop()
        {
            if (Listener != null)
            {
                Listener.Abort();
                Listener.Close();
                Listener = null;
            }
            DataSender.Exit();
        }

        private void WebRequestCallback(IAsyncResult result)
        {
            if (Listener == null)
                return;

            HttpListenerContext Context = this.Listener.EndGetContext(result);
            Listener.BeginGetContext(new AsyncCallback(WebRequestCallback), this.Listener);
            ProcessRequest(Context);
        }
     
        //这里进行转发
        private void ProcessRequest(System.Net.HttpListenerContext Context)
        {
            Stream request = Context.Request.InputStream;

            string input = null;
            using (StreamReader sr = new StreamReader(request))
            {
                input = sr.ReadToEnd();
            }
            string strData = input.Split('\n')[3];
            //浏览器重启，读取一次终端ip，更新client连接
            if (strData.StartsWith("WebStart"))
            {
                DataSender.RenewClient();
            }else
                DataSender.PushUrl(input.Split('\n')[3]);
        }
    }  
 
}
