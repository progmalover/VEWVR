using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace VEWebVideoService
{
    public class HttpServer
    {
        protected System.Net.HttpListener Listener;
        protected bool IsStarted = false;

        
        public void Start(string strUrl)
        {
            if (IsStarted) //已經再Listen就直接Return  
                return;

            if (Listener == null)
                Listener = new HttpListener();

          
            Listener.Prefixes.Add(strUrl);

            IsStarted = true;
            Listener.Start(); 

            
            IAsyncResult result = this.Listener.BeginGetContext(
            new AsyncCallback(WebRequestCallback), this.Listener);
        }

        public void Stop()
        {
            if (Listener != null)
            {
                Listener.Close();
                Listener = null;
                IsStarted = false;
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

        private void ProcessRequest(System.Net.HttpListenerContext Context)
        {
            Stream request = Context.Request.InputStream;

            string input = null;
            using (StreamReader sr = new StreamReader(request))
            {
                input = sr.ReadToEnd();
            }
            Console.WriteLine(input);
        }
    }  
 
}
