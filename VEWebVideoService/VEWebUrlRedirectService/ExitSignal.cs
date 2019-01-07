using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
/*
* Notice
* This code is designed for vesystem,all right reserved. --maxwell.z
* 2018.12
*/
namespace VEWebUrlRedirectService
{
    class ExitSignal
    {
        protected AutoResetEvent ExitEvent;
        public ExitSignal()
        {
            ExitEvent = new AutoResetEvent(true);
        }
        public void Wait()
        {
            ExitEvent.WaitOne();
        }


        static ExitSignal s_signal = new ExitSignal();
        static public void Join()
        {
            s_signal.Wait();
        }
    }
}
