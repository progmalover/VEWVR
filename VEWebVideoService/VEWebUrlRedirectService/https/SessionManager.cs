using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
/*
* Notice
* This code is designed for vesystem,all right reserved. --maxwell.z
* 2018.12
*/

namespace VEWebUrlRedirectService.https
{
    class SSLSessionManager
    {
        public SSLSessionManager(HttpsBaseServer baseServer)
        {
            mBaseServer = baseServer;
            bStarted = false;
            mLocker = new object();
            mSessions = new Queue<SSLSession>();
            mWaitEvent = new AutoResetEvent(false);
            mExitEvent = new AutoResetEvent(false);
            ThreadPool.SetMaxThreads(7, 7);
            ThreadPool.SetMinThreads(2, 2);
        }

        public void EnSession(SSLSession s)
        {
            lock (mLocker)
            {
                 mSessions.Enqueue(s);
                 mWaitEvent.Set();
            }
        }
        public SSLSession OutSession()
        {
            SSLSession ss=null;
            lock(mLocker)
            {
                if (mSessions.Count > 0)
                    ss = mSessions.Dequeue();
            }
            return ss;
        }

        protected void ProcessSession(SSLSession ss)
        {

            if (ss.IsAlive())
            {
                ss.PrcessRequest();
                this.EnSession(ss);
            }
            else
            {
                ss.Recycle();
                ss = null;
            }
        }

        protected void Process()
        {
            while (bStarted)
            {
                while(mSessions.Count <= 0)
                {
                    System.Diagnostics.Debug.Write("\n****无连接处理，进入等待。。\n");
                    mWaitEvent.WaitOne();
                }
                int iCount = mSessions.Count;
                for (int i = 0; i < iCount; i++)
                {
                    SSLSession ss = OutSession();
                    if (ss != null)
                    {
                        ThreadPool.QueueUserWorkItem(new WaitCallback((object obj) =>
                        {
                            SSLSession ssls = (SSLSession)obj;
                            ProcessSession(ssls);
                        }),ss);
                    }
                    else
                        break;
                }

                Thread.Sleep(2000);
            }
        }

        internal void WaitExit()
        {
            bStarted = false;
            mExitEvent.WaitOne();
        }

        public void Start()
        {
            if (!bStarted)
            {
                bStarted = true;
                ThreadPool.QueueUserWorkItem(new WaitCallback((object obj) =>
                {
                    Process();
                    mExitEvent.Set();
                }));
            }
        }

        #region  variants
        private Object mLocker;
        private Queue<SSLSession> mSessions;
        private HttpsBaseServer mBaseServer;
        private AutoResetEvent mWaitEvent;
        private AutoResetEvent mExitEvent;
        private bool bStarted;
        #endregion
    }
}
