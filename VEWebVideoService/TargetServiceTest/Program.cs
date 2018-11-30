using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Threading;
using TerminalWebVideoService;

namespace TargetServiceTest
{
    class Program
    {
        static void Main(string[] args)
        {
            //cpListener listener = new TcpListener(IPAddress.Parse("127.0.0.1"), 1886);
            Service service = new Service();
            service.Start();
            if (System.Console.Read() == 'q')
                service.Stop();
        }
    }
}
