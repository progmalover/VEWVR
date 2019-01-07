using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
/*
 * Notice
 * This code is designed for vesystem,all right reserved. --maxwell.z
 * 2018.12
 */
namespace VEWebUrlRedirectService.OpensslWrapper
{
    class SslContext : Base
    {
        static bool s_bsslInit = false;
        //默认支持https
        public SslContext() 
        {
            try
            {
                if (s_bsslInit == false)
                {
                    Native.OPENSSL_init_ssl(Native.OPENSSL_INIT_SSL_DEFAULT,IntPtr.Zero);
                    Native.ERR_load_SSL_strings();
                    s_bsslInit = true;
                }
                
                IntPtr hssl = CreateCtx();
                if (hssl == IntPtr.Zero)
                {
                    Log.Logger.Instance.WriteLog("ssl 初始化失败！");
                    return;
                }
                base.Construct(hssl, true);

                int ret = (int)SetOptions((Native.SSL_OP_NO_TLSv1 | GetOptions()));  //(int)Native.SSL_CTX_set_options(this.sslCtx.Handle, Native.SSL_OP_NO_TLSv1 | Native.SSL_CTX_get_options(this.sslCtx.Handle));
                if (ret == 0)
                {
                    Log.Logger.Instance.WriteLog("SSL_CTX_set_options failed!");
                    return;
                }

                string basePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                LoadCertAndKey(basePath + "\\domain.crt", basePath + "\\domain.key");
            }
            catch (Exception e)
            {
                Log.Logger.Instance.WriteException(e);
                s_bsslInit = false;
            }
        }

        private bool LoadCertAndKey(string cert, string key)
        {
            try
            {
                int ret = this.UseCertFile(cert, Native.SSL_FILETYPE_PEM);// Native.SSL_CTX_use_certificate_file(this.sslCtx.Handle, cert, Native.SSL_FILETYPE_PEM);
                if (ret != 1)
                {
                    Log.Logger.Instance.WriteLog("载入证书文件错误！");
                    return false;
                }
                ret = this.UsePKFile(key, Native.SSL_FILETYPE_PEM);// Native.SSL_CTX_use_PrivateKey_file(this.sslCtx.Handle, key, Native.SSL_FILETYPE_PEM);
                if (ret != 1)
                {
                    Log.Logger.Instance.WriteLog("载入秘钥错误！");
                    return false;
                }
                ret = this.CheckPK();// Native.SSL_CTX_check_private_key(this.sslCtx.Handle);
                if (ret != 1)
                {
                    Log.Logger.Instance.WriteLog("秘钥和证书不匹配！");
                    return false;
                }
            }
            catch (Exception e)
            {
                throw e;
            }

            return true;
        }

        private IntPtr CreateCtx()
        {
            IntPtr[] methods = { SslMethod.SSL_server_method.Handle, SslMethod.SSLv1_server_method.Handle,
            SslMethod.SSLv11_server_method.Handle,SslMethod.SSLv12_server_method.Handle};
            IntPtr hssl = IntPtr.Zero;
            for (int i = 0; i < methods.Length; i++)
            {
                hssl = Native.SSL_CTX_new(methods[i]);
                if (hssl != IntPtr.Zero)
                    break;
            }
            return hssl;
        }
        #region API
        internal ulong SetOptions(ulong opt)
        {
            return Native.SSL_CTX_set_options(ptr, opt);
        }

        internal ulong GetOptions()
        {
            return Native.SSL_CTX_get_options(ptr);
        }

        internal int UseCertFile(string cert, int ftype)
        {
            return Native.SSL_CTX_use_certificate_file(ptr, cert, ftype);
        }

        internal int UsePKFile(string key, int ftype)
        {
            return Native.SSL_CTX_use_PrivateKey_file(ptr, key, ftype);
        }

        internal int CheckPK()
        {
            return Native.SSL_CTX_check_private_key(ptr);
        }
        #endregion
        static SslContext Instance;
        public static SslContext GetInst(){
            if(Instance == null)
                Instance = new SslContext();
            if (!s_bsslInit)
            {
                Instance = null;
                Log.Logger.Instance.WriteLog("初始化失败！");
            }

            if(Instance.Handle == IntPtr.Zero)
            {
                Instance = null;
                Log.Logger.Instance.WriteLog("初始化失败！");
            }
            return Instance;
        }
        protected override void OnDispose()
        {
            //throw new NotImplementedException();
        }

        
    }
}
