using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VEWebVideoService;

namespace HttpTest
{
    class Program
    {
        static HttpServer hs = new HttpServer();
        static void Main(string[] args)
        {
            hs.Start("http://localhost:8233/");

            var c = 0;
            while ((c = System.Console.Read()) != 'q')
            {
                System.Threading.Thread.Sleep(5000); 
            }

            hs.Stop();
        }
    }
}
