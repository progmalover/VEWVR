using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VEWebUrlRedirectService
{
    class DnsInfoLoader
    {
        public DnsInfoLoader()
        {

        }

        public bool Load()
        {
            return true;
        }

        ~DnsInfoLoader()
        {

        }
    }

    //修改系统dns转向
    class DnsController
    {
        public bool LoadDnsToSystem()
        {
            DnsInfoLoader loader = new DnsInfoLoader();
            if (false == loader.Load())
                return false;
            return UpdateDnsSystem(loader);
           // return true;
        }

        protected bool UpdateDnsSystem(DnsInfoLoader dnsInfo)
        {
            return true;
        }
    }
}
