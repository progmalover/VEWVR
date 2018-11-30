using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace VEWebUrlRedirectService
{
    public class LocalResourceManager
    {
        string SourceDirPath = "";
        public LocalResourceManager()
        {
            SourceDirPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (!SourceDirPath.EndsWith("/") && !SourceDirPath.EndsWith("\\"))
            {
                SourceDirPath += "\\";// for dos path
            }
        }
        //转换unicode路径
        string  ConvertPath(string rawURL)
        {
            if (rawURL.IndexOf("%") < 0)
                return rawURL;

            return Uri.UnescapeDataString(rawURL);
        }

        protected string PathMap(string rawURL)
        {
            string unescUrl = ConvertPath(rawURL);
            
            string beginToken = unescUrl[0].ToString();
            string strFullPath = SourceDirPath;
            //map to root file or insert source dir
            if (unescUrl == beginToken)
                strFullPath += "Source" + beginToken + "test.html";
            else
                strFullPath +="Source" + unescUrl;

            return strFullPath;
        }

        public FileStream GetFStream(string rawUrl ,ref bool bBlockFile )
        {
            string []raw_exts = new string[]{ ".gif", ".bmp", ".png", ".jpg" };
            bBlockFile = false;
            string localPath = PathMap(rawUrl);
            try
            {
                if (File.Exists(localPath))
                {
                    string lcPath = localPath.ToLower();
                    foreach( var ext in raw_exts)
                    {
                        if (lcPath.Contains(ext))
                        {
                            bBlockFile = true;
                            break;
                        }
                    }
                  
                    return File.Open(localPath, FileMode.Open, FileAccess.Read);
                }
            }
            catch (Exception e)
            {
                Log.Logger.Instance.WriteException(e);
            }
            return null;
        }
    }

    public class HttpServer
    {
        protected HttpListener Listener;
        protected AutoResetEvent ExitEvent;
        protected LocalResourceManager LocalResource;
        public HttpServer()
        {
            Listener = new HttpListener();
            ExitEvent = new AutoResetEvent(false);
            LocalResource = new LocalResourceManager();
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

            }
            catch (Exception e)
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
//"/test_files/bootstrap.min.js.%C3%A4%C2%B8%C2%8B%C3%A8%C2%BD%C2%BD"
        private void OnResponse(System.Net.HttpListenerResponse response ,System.Net.HttpListenerRequest request)
        {
            String reqStrURlRaw = request.RawUrl;

            bool bBlockFile = false;
            FileStream fr = LocalResource.GetFStream(reqStrURlRaw,ref bBlockFile);
            if (null == fr)
            {
                return;
            }

            BinaryWriter bw = new BinaryWriter(response.OutputStream);
            StreamWriter sw = new StreamWriter(response.OutputStream);
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

                datas = null;
            }
            catch{ }

            bw.Close();
            sw.Close();
            fr.Close();
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

            System.Diagnostics.Debug.Write(input);

            OnResponse(Context.Response,Context.Request);
           
        }

    }
}
