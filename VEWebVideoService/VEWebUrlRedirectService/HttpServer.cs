using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
/*
* Notice
* This code is designed for vesystem,all right reserved. --maxwell.z
* 2018.12
*/

namespace VEWebUrlRedirectService
{
    public class HttpServer
    {
        protected HttpListener Listener;
   
        protected LocalResourceManager LocalResource;
       // protected TcpDataSender DataSender;
        public HttpServer()
        {
           // DataSender = TcpDataSender.GetInst();
            Listener = new HttpListener();
            LocalResource = new LocalResourceManager();
        }

        //start httpserv
        public void Start(string strPrefix)
        {
            try
            {
                if (Listener == null)
                    Listener = new HttpListener();
                if (!Listener.IsListening)
                {
                    Listener.Prefixes.Add(strPrefix);
                    Listener.Start();
                    IAsyncResult result = Listener.BeginGetContext(
                    new AsyncCallback(WebRequestCallback), this.Listener);
                }

            }
            catch (Exception e)
            {
                Log.Logger.Instance.WriteException(e);
            }
        }

        public void Stop()
        {
            if (Listener != null)
            {
                Listener.Abort();
                Listener.Close();
                Listener = null;
            }
            
        }
       
        private void WebRequestCallback(IAsyncResult result)
        {
            if (Listener == null)
                return;

            HttpListenerContext Context = this.Listener.EndGetContext(result);
            Listener.BeginGetContext(new AsyncCallback(WebRequestCallback), this.Listener);
            ProcessRequest(Context);
        }

        virtual protected BinaryReader CreateBinaryStreamReader(Stream rawStream)
        {
            return new BinaryReader(rawStream);
        }

        virtual protected StreamReader CreateStringStreamReader(Stream rawStream)
        {
            return new StreamReader(rawStream);
        }

        virtual protected BinaryWriter CreateBinaryStreamWriter(Stream rawStream)
        {
            return new BinaryWriter(rawStream);
        }

        virtual protected StreamWriter CreateStringStreamWriter(Stream rawStream)
        {
            return new StreamWriter(rawStream);
        }

        private void OnResponse(System.Net.HttpListenerResponse response ,System.Net.HttpListenerRequest request)
        {
            String reqStrURlRaw = request.Url.ToString();
            
            if (!LocalResource.LocalFileExist(reqStrURlRaw) && request.UrlReferrer == null)
            {
                reqStrURlRaw = "/";
            } else
                reqStrURlRaw = LocalResource.UrlToLocalRef(reqStrURlRaw, request.UrlReferrer.ToString());
            if (!LocalResource.LocalFileExist(reqStrURlRaw))
                reqStrURlRaw = "/";
            bool bBlockFile = false;
            FileStream fr = LocalResource.GetFStream(reqStrURlRaw,ref bBlockFile);
            if (null == fr)
            {
                request.InputStream.Close();
                return;
            }

            BinaryWriter bw = CreateBinaryStreamWriter(response.OutputStream);
            StreamWriter sw = CreateStringStreamWriter(response.OutputStream);
            try
            {
                byte[] datas = new byte[4096];
                int rlen = 0;
                while ((rlen = fr.Read(datas, 0, datas.Length)) > 0)
                {
                    if (bBlockFile)
                    {
                        bw.Write(datas);
                    }
                    else
                    {
                        Encoding utf8 = Encoding.UTF8;
                        Encoding unicode = Encoding.Unicode;
                        byte[] dataPtr = datas;
                        if(rlen < 4096)
                        {
                            dataPtr = new byte[rlen];
                            Array.Copy(datas , dataPtr, rlen);
                        }
                        byte[] uniBytes = Encoding.Convert(utf8, unicode, dataPtr);
                        Char[] uniChars = new Char[unicode.GetCharCount(uniBytes, 0, uniBytes.Length)];
                        unicode.GetChars(uniBytes, 0, uniBytes.Length, uniChars, 0);
                        sw.Write(new string(uniChars));
                        dataPtr = null;
                    }
                }

                sw.Flush();
                bw.Flush();
                bw.Close();
                sw.Close();
                datas = null;
            }
            catch{ }

            bw = null;
            sw = null;
            fr.Close();
            fr = null;
        }
        
        //这里进行转发
        protected virtual void ProcessRequest(System.Net.HttpListenerContext Context)
        {
            Stream request = Context.Request.InputStream;
            OnResponse(Context.Response,Context.Request);
        }

    }


}
