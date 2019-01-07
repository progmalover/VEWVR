using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
 
using System.Diagnostics;
using System.Text;
using System.Runtime.InteropServices;
using VEWebUrlRedirectService.OpensslWrapper;
using System.Reflection;
using VEWebUrlRedirectService.https;
/*
* Notice
* This code is designed for vesystem,all right reserved. --maxwell.z
* 2018.12
*/
namespace VEWebUrlRedirectService
{

    class HttpsServer : HttpsBaseServer
    {
        protected LocalResourceManager LocalResource;
        public HttpsServer(int port):base(port)
        {
            LocalResource = new LocalResourceManager();
        }
        public override void OnResponse(HttpResponse response,HttpRequest request)
        {
             System.Diagnostics.Debug.Write("HttpsServer:OnResponse!\n");
            TcpDataSender.GetInst().PushUrl(request.mFullURL);

            String reqStrURlRaw = request.mRawURL.ToString();
            bool bSourceExist = LocalResource.LocalFileExist(reqStrURlRaw);
            if (!bSourceExist && request.mReferer == null)
            {
                reqStrURlRaw = "/";
            }
            else
            if (request.mReferer != null)
            {
                string strRef = "";
                if (request.mReferer.StartsWith("https:"))
                    strRef = request.mReferer.Substring(8);
                if (request.mReferer.StartsWith("http:"))
                    strRef = request.mReferer.Substring(7);
                reqStrURlRaw = LocalResource.UrlToLocalRef(request.mFullURL, strRef);
            }

            if (!LocalResource.LocalFileExist(reqStrURlRaw))
                reqStrURlRaw = "/";
            bool bBlockFile = false;
            FileStream fr = LocalResource.GetFStream(reqStrURlRaw, ref bBlockFile);
            if (null == fr)
            {
                //request.();
                response.SetStatus(404);
                try
                {
                    response.WriteString("<html><body>文件未找到！</body></html>");
                }
                catch {}
                return;
            }

            try
            {
                byte[] datas = new byte[40960];
                int rlen = 0;
                while ((rlen = fr.Read(datas, 0, datas.Length)) > 0)
                {
                    try
                    {
                        if (bBlockFile)
                        {
                                response.Write(datas, rlen);
                        }
                        else
                        {
                                response.Write(datas,rlen,true);
                        }  
                    }
                    catch(IOException e) {
                        Log.Logger.Instance.WriteLog(e.Message);
                        break;
                    }
                }
                
                datas = null;
            }
            catch { }

            System.Diagnostics.Debug.Write("HttpsServer:OnResponse end!\n");
        }
    }

}