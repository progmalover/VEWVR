using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
 
namespace TerminalWebVideoService
{
    class Service
    {
        private volatile bool  _runing;
        private Thread _mainThread;
        TcpListener _listener = null;
        public Service()
        {
            _runing = false;
            _mainThread = null;
        }
        public void Start()
        {
            if (null == _mainThread)
                _mainThread = new Thread(DoLoop);
            _mainThread.Start();
        }

        public void DoLoop()
        {
            _runing = true;
            //如果绑定失败，不能退出主程序，做持续绑定动作
            while (_runing)
            {
                try
                {
                    if (_listener != null)
                    {
                        _listener.Stop();
                        _listener = null;
                    }
                    _listener = new TcpListener(IPAddress.Parse("0.0.0.0"),44223);
                    _listener.Start();
                    break;
                } catch (Exception e)
                {
                    Log.Logger.Instance.WriteLog(e.Message);
                    Thread.Sleep(2000);
                }
            }

            //accepting loop
            while (_runing)
            {
                TcpClient client = null;
                try
                {
                    client = _listener.AcceptTcpClient();
                    
                    if (!_runing)
                        continue;
                }
                catch
                {
                    Thread.Sleep(2000);
                    continue;
                }
               
                new Thread((param) =>
                {
                    TcpClient _client = (TcpClient)param;
                    StreamReader sr = null; 
                    string line = null;
                    while (_runing)
                    {
                        try
                        {
                           
                            sr = new StreamReader(_client.GetStream());
                           
                            while ((line = sr.ReadLine()) != null)
                            {
                                Log.Logger.Instance.WriteLog(line);
                                if (line.Trim().StartsWith("http"))
                                {
                                    try
                                    {
                                        InvokeBrowser.CallWebBrowser(line);
                                    }
                                    catch (Exception e)
                                    {
                                        Log.Logger.Instance.WriteLog(e.Message);
                                        break;
                                    }
                                }
                            }
                            sr.Close();
                            sr = null;

                        }
                        catch (Exception e)
                        {
                            Log.Logger.Instance.WriteLog(e.Message);
                            break;
                        }
                    }
                    if (null != sr)
                        sr.Close();
                    _client.Close();
                }

                ).Start(client);

            }
        }

        public void Stop()
        {
            _runing = false;
            if (null != this._listener)
                _listener.Stop();
            //main thread必定存在，必须等待
            try
            {
                if (false == _mainThread.Join(5000))
                {
                    try
                    {
                        _mainThread.Abort();
                    }
                    catch {} 
                }
            }
            catch { }
            
            _mainThread = null;
        }

        public void Join()
        {
            _mainThread.Join();
        }
    }

}
