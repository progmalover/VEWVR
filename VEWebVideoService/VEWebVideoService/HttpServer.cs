using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
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
        protected volatile Thread mWorkThread;
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

        public bool ConnectClient()
        {
            string tmpIp = "";
            int tmpPort = 0;
            //未连接返回
            if (client.Connected)
                return true;
            try
            {
                QueryIpAddress(ref tmpIp, ref tmpPort);
             
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
                    return false;
                }
               
            }
            catch (Exception e)
            {
                Log.Logger.Instance.WriteLog(e.Message);
                return false;
            }
            return true;
        }

        public void Start()
        {
            //产生异常后，模块可以重入
            if (mWorkThread != null)
                return;

            mWorkThread = new Thread(() =>
            {
                bExit = false;

                while (!bExit)
                {
                    while (mque.Count <= 0)
                    {
                        //wait for message - 使用event机制
                        waitEvent.WaitOne();
                        if (bExit)
                            return;
                    }

                    if (ConnectClient() == false)
                    {
                        mque.Clear();//清理掉缓冲
                        Thread.Sleep(2000);//2秒进行一次重连
                        continue;
                    }
                      
                    //读写保护
                    string strData = null;
                    lock (mLock)
                    {
                        //只发送当前第一个消息，其它的不做处理
                        strData = (string)mque.Dequeue();
                        mque.Clear();
                    }
                       
                    try
                    {
                        BinaryWriter bw = new BinaryWriter(client.GetStream());
                        byte[] bytes = Encoding.UTF8.GetBytes(strData);
                        bw.Write(bytes, 0, bytes.Length);
                        bw.Flush();
                        bw = null;
                    }
                    catch
                    {
                        //网络对象清理
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
            });
            mWorkThread.Start();
        }
        //推送url到终端
        public void PushUrl(string strUrl)
        {
            lock(mLock)
            {
                mque.Enqueue(strUrl);
                waitEvent.Set();
            }

            Start();
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
        protected AutoResetEvent ExitEvent;
        public HttpServer()
        {
            DataSender = new TcpDataSender();
            Listener =   new HttpListener();
            ExitEvent = new AutoResetEvent(false);
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

                DataSender.Start();
            }
            catch(Exception e)
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
            ExitEvent.Reset();
        }

        public void Join()
        {
            //DataSender.Join();
            ExitEvent.WaitOne();
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
            if (strData.StartsWith("http"))
                DataSender.PushUrl(strData);
        }
    }  
 
}
