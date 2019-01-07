using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using VEWebUrlRedirectService.OpensslWrapper;
/*
* Notice
* This code is designed for vesystem,all right reserved. --maxwell.z
* 2018.12
*/

namespace VEWebUrlRedirectService.https
{
    class SSLSession : IDisposable
    {
        protected Socket tcpSocket;
        protected OpenSslServerStream sslStream;
        protected ServerListener Listener;
        protected long　mKeepAliveTime;
        protected bool  mbKeepAlive;
        protected bool  mbNeedClose;
        public SSLSession(ref Socket tcpSocket, ServerListener listener)
        {
            this.tcpSocket = tcpSocket;
            this.Listener = listener;
          
            mKeepAliveTime = DateTime.Now.Ticks;
            mbKeepAlive = false;
            mbNeedClose = false;

        }

        //没有keepalive，接连持续半分钟，keepalive持续1.5分钟
        public void PrcessRequest()
        {
            //preauth
            if (sslStream == null)
            {  
                string basePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                sslStream = new OpenSslServerStream(ref tcpSocket);
            }
            //start http
            string strRequest = sslStream.Read();
            if (strRequest.Length <= 0)
            {
                return;
            }
            HttpRequest request = new HttpRequest( strRequest);
            HttpResponse response = new HttpResponse(sslStream ,ref request);
            if (Listener != null)
                Listener.OnRequest(request, response);
            response.CommitData();
            mbKeepAlive = request.mIsKeepAlive;
            mbNeedClose = request.mNeedClose;
            mKeepAliveTime = DateTime.Now.Ticks;
           
        }

        public void Recycle()
        {
            System.Diagnostics.Debug.WriteLine(string.Format("recycle handle {0}\n", tcpSocket.Handle.ToInt32()));
            
            if (tcpSocket != null && tcpSocket.Handle != null && tcpSocket.Connected)
                tcpSocket.Close();
            if (sslStream != null)
                sslStream.Close();
            tcpSocket = null;
            sslStream = null;
        }
        public bool IsAlive()
        {
            //客户端需要关闭
            if (mbNeedClose)
                return false;

            long esctime = DateTime.Now.Ticks - mKeepAliveTime;
            if(this.mbKeepAlive && (esctime > 900000000))
            {
                System.Diagnostics.Debug.Write("连接(90s)超时退出！\n");
                return false;
            }

            if (!this.mbKeepAlive && (esctime > 300000000))
            {
                System.Diagnostics.Debug.Write("连接(30s)完成退出！");
                return false;
            }

            if (tcpSocket == null || tcpSocket.Handle == null ||tcpSocket.Connected == false)
            {
                System.Diagnostics.Debug.Write("连接已经退出！");
                return false;
            }

            return true;
        }

        void IDisposable.Dispose()
        {
             
        }
    }

}
