using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace VEWebUrlRedirectService
{
    class HttpWebSocketServer :HttpServer
    {
        protected string ParseURLDataFromAjax(string strData)
        {
            /*
            if (!strData.Contains("------WebKitFormBoundary"))
                return "";
                */
            int bStart = strData.IndexOf("http");
            int bEnd = bStart;
            string strUrl="";
            char ch = '0';
            while ((ch=strData[bEnd++]) != '\r')
            {
                strUrl += ch;
            }
            
            return strUrl;
        }

        protected override void ProcessRequest(System.Net.HttpListenerContext Context)
        {
            Stream request = Context.Request.InputStream;

            if (!(request == Stream.Null))
            {
                string input = null;
                using (StreamReader sr = CreateStringStreamReader(request))
                {
                    input = sr.ReadToEnd();
                }

                System.Diagnostics.Debug.Write(input);
                //转发url
                string strUrl = ParseURLDataFromAjax(input);
                if (strUrl.StartsWith("http"))
                {
                    TcpDataSender.GetInst().PushUrl(input);
                }
            }
            Context.Response.StatusCode = 200;
            Context.Response.StatusDescription = "Ok";
            Context.Response.Close();
        }
    }
}
