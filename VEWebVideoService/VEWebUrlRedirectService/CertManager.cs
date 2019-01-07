using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace VEWebUrlRedirectService
{
    class CertManager
    {
        protected X509Certificate serverCertificate = null;
        public CertManager()
        {
            X509Store store = new X509Store(StoreName.My);
            store.Open(OpenFlags.ReadWrite);
            // 检索证书 
            X509Certificate2Collection certs = store.Certificates.Find(X509FindType.FindBySubjectName, "wust", false); // vaildOnly = true时搜索无结果。
            if (certs.Count == 0)
            {
                throw new Exception("not found cert!");
            }
               
            //取第一个
            serverCertificate = certs[0];
        }

        static CertManager s_ctmanager;
        public static CertManager Instance()
        {
            if (null == s_ctmanager)
                s_ctmanager = new CertManager();
            return s_ctmanager;
        }
    }
}
