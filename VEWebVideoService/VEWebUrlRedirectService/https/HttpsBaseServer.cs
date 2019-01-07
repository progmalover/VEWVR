using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using VEWebUrlRedirectService.OpensslWrapper;
/*
* Notice
* This code is designed for vesystem,all right reserved. --maxwell.z
* 2018.12
*/

namespace VEWebUrlRedirectService.https
{
   
    class HttpsBaseServer : ServerListener,IDisposable
    {
        #region Methods
        public HttpsBaseServer(int port)
        {
            //pre-initlize
            sessionManager = new SSLSessionManager(this);
            SslContext.GetInst();

            //start bind
            try
            {
                tcpListener = new TcpListener(IPAddress.Parse("0.0.0.0"), port);
            }
            catch
            {
                StringBuilder strb = new StringBuilder();
                strb.AppendFormat("bind port {0} failed!", port);
                Log.Logger.Instance.WriteLog(strb.ToString());
            }
            
        }

        public bool Start()
        {
            if (tcpListener == null)
                return false;

            if (null != mainThread)
                return true;

            tcpListener.Start();

            AcceptLoop();

            sessionManager.WaitExit();
            return true;
        }

        protected void AcceptLoop()
        {
            bAlive = true;
            while (bAlive)
            {
                Socket tcpSocket = null;
                try
                {
                    tcpSocket = tcpListener.AcceptSocket();
                    if (tcpSocket == null)
                        continue;
                    tcpSocket.ReceiveTimeout = 2000;//2秒超时等待
                    System.Diagnostics.Debug.Write("tcpListener.AcceptSocket() 1!\n");
                    sessionManager.EnSession(new SSLSession(ref tcpSocket, this));
                    sessionManager.Start();
                }
                catch (Exception e)
                {
                    Log.Logger.Instance.WriteException(e);
                }
            }
        }

        public void Stop()
        {
            if (!bAlive)
            {
                return;
            }
            bAlive = false;
        }

        void IDisposable.Dispose()
        {
            throw new NotImplementedException();
        }
        public virtual void OnResponse( HttpResponse response, HttpRequest request)
        {

        }

        public override void OnRequest(HttpRequest request, HttpResponse response)
        {
            //throw new NotImplementedException();
            //OpenSslServerStream stream = response.GetStream();
            //stream.WriteString("HTTP/1.1 200 OK");
            OnResponse(response, request);
        }
        #endregion
        #region parameters
        protected TcpListener tcpListener;
        protected Thread mainThread;
        protected volatile bool bAlive;
        protected SSLSessionManager sessionManager;
        #endregion
    }
}
