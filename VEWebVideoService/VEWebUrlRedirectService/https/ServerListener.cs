using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
/*
* Notice
* This code is designed for vesystem,all right reserved. --maxwell.z
* 2018.12
*/

namespace VEWebUrlRedirectService.https
{
    abstract class ServerListener
    {
        public abstract void OnRequest(HttpRequest request, HttpResponse response);
    }
}
