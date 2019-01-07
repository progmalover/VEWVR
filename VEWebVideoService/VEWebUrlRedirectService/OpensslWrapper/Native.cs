using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
/*
* Notice
* The origin version of this file get from openssl.net project on github.
* i modified it according to project requirment. --maxwell.z
* 2018.12
*/
namespace VEWebUrlRedirectService.OpensslWrapper
{
    internal class Native
    {


        /* OPENSSL_INIT flag 0x010000 reserved for internal use */
        public const ulong OPENSSL_INIT_NO_LOAD_SSL_STRINGS = 0x00100000L;
        public const ulong OPENSSL_INIT_LOAD_SSL_STRINGS = 0x00200000L;
        public const ulong OPENSSL_INIT_LOAD_CRYPTO_STRINGS = 0x00000002L;
        public const ulong OPENSSL_INIT_SSL_DEFAULT = 
        (OPENSSL_INIT_LOAD_SSL_STRINGS | OPENSSL_INIT_LOAD_CRYPTO_STRINGS);

        // file type
        public const int SSL_FILETYPE_ASN1 = 2;
        public const int SSL_FILETYPE_PEM = 1;

        //option
        public const int SSL_OP_NO_SSLv2 = 0x00000000;
        public const int SSL_OP_NO_SSLv3 = 0x02000000;
        public const int SSL_OP_NO_TLSv1 = 0x04000000;
        public const int SSL_OP_NO_TLSv1_2 = 0x08000000;
        public const int SSL_OP_NO_TLSv1_1 = 0x10000000;
        public const int SSL_OP_NO_DTLSv1 = 0x04000000;
        public const int SSL_OP_NO_DTLSv1_2 = 0x08000000;
        //const string basePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        const string SSLDLLNAME = "libsslMD.dll";
        #region Initialization

        [DllImport( SSLDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public extern static int ERR_load_SSL_strings();

        [DllImport(SSLDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public extern static ulong SSL_CTX_set_options(IntPtr handle, ulong p);
        [DllImport(SSLDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public extern static ulong SSL_CTX_get_options(IntPtr handle);

        [DllImport(SSLDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public extern static int SSL_set_fd(IntPtr handle1, IntPtr handle2);
        

        [DllImport(SSLDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public extern static int OPENSSL_init_ssl(ulong opt ,IntPtr ptr);

        #endregion

        #region SSL Methods

        [DllImport(SSLDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public extern static IntPtr SSLv2_method();

        internal static IntPtr ExpectNonNull(IntPtr intPtr)
        {
            throw new NotImplementedException();
        }


        [DllImport(SSLDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public extern static IntPtr SSLv23_server_method();

        [DllImport(SSLDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public extern static IntPtr SSLv23_client_method();

 　
        [DllImport(SSLDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public extern static IntPtr TLS_server_method();

        [DllImport(SSLDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public extern static IntPtr TLSv1_server_method();

        [DllImport(SSLDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public extern static IntPtr TLSv1_1_server_method();

        [DllImport(SSLDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public extern static IntPtr TLSv1_2_server_method();


        #endregion

        #region SSL_CTX

        [DllImport(SSLDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public extern static IntPtr SSL_CTX_new(IntPtr sslMethod);

        [DllImport(SSLDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public extern static void SSL_CTX_free(IntPtr ctx);

        [DllImport(SSLDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public extern static int SSL_CTX_ctrl(IntPtr ctx, int cmd, int arg, IntPtr parg);

      
        [DllImport(SSLDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public extern static void SSL_CTX_set_cert_store(IntPtr ctx, IntPtr cert_store);

        public const int SSL_VERIFY_NONE = 0x00;
        public const int SSL_VERIFY_PEER = 0x01;
        public const int SSL_VERIFY_FAIL_IF_NO_PEER_CERT = 0x02;
        public const int SSL_VERIFY_CLIENT_ONCE = 0x04;

        [DllImport(SSLDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public extern static void SSL_CTX_set_verify_depth(IntPtr ctx, int depth);

        [DllImport(SSLDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public extern static void SSL_CTX_set_client_CA_list(IntPtr ctx, IntPtr name_list);

        [DllImport(SSLDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public extern static IntPtr SSL_CTX_get_client_CA_list(IntPtr ctx);

        [DllImport(SSLDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public extern static int SSL_CTX_load_verify_locations(IntPtr ctx, string file, string path);

        [DllImport(SSLDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public extern static int SSL_CTX_set_default_verify_paths(IntPtr ctx);

        [DllImport(SSLDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public extern static int SSL_CTX_set_cipher_list(IntPtr ctx, string cipher_string);

        [DllImport(SSLDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public extern static int SSL_CTX_use_certificate_chain_file(IntPtr ctx, string file);

        [DllImport(SSLDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public extern static int SSL_CTX_use_certificate(IntPtr ctx, IntPtr cert);

        [DllImport(SSLDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public extern static int SSL_CTX_use_PrivateKey(IntPtr ctx, IntPtr pkey);

        [DllImport(SSLDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public extern static int SSL_CTX_use_PrivateKey_file(IntPtr ctx, string file, int type);

        [DllImport(SSLDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public extern static int SSL_CTX_use_certificate_file(IntPtr ctx, string file, int type);
        
         

        [DllImport(SSLDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public extern static int SSL_CTX_check_private_key(IntPtr ctx);

        public const int SSL_MAX_SID_CTX_LENGTH = 32;

        [DllImport(SSLDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public extern static int SSL_CTX_set_session_id_context(IntPtr ctx, byte[] sid_ctx, uint sid_ctx_len);

        [DllImport(SSLDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public extern static void SSL_CTX_set_default_passwd_cb_userdata(IntPtr ssl, IntPtr data);

        

        #endregion

        #region SSL functions

        [DllImport(SSLDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public extern static string SSL_CIPHER_description(IntPtr ssl_cipher, byte[] buf, int buf_len);

        [DllImport(SSLDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public extern static IntPtr SSL_CIPHER_get_name(IntPtr ssl_cipher);

        [DllImport(SSLDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public extern static int SSL_CIPHER_get_bits(IntPtr ssl_cipher, out int alg_bits);

        [DllImport(SSLDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public extern static IntPtr SSL_CIPHER_get_version(IntPtr ssl_cipher);

        [DllImport(SSLDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public extern static IntPtr SSL_get_current_cipher(IntPtr ssl);

        [DllImport(SSLDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public extern static IntPtr SSL_get_ciphers(IntPtr ssl);

        [DllImport(SSLDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public extern static int SSL_get_verify_result(IntPtr ssl);

        [DllImport(SSLDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public extern static int SSL_set_verify_result(IntPtr ssl, int v);

        [DllImport(SSLDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public extern static IntPtr SSL_get_peer_certificate(IntPtr ssl);

        [DllImport(SSLDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public extern static int SSL_get_error(IntPtr ssl, int ret_code);

        [DllImport(SSLDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public extern static int SSL_accept(IntPtr ssl);

        [DllImport(SSLDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public extern static int SSL_shutdown(IntPtr ssl);

        [DllImport(SSLDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public extern static int SSL_write(IntPtr ssl, byte[] buf, int len);

        [DllImport(SSLDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public extern static int SSL_read(IntPtr ssl, byte[] buf, int len);

        [DllImport(SSLDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public extern static int SSL_renegotiate(IntPtr ssl);

        [DllImport(SSLDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public extern static int SSL_set_session_id_context(IntPtr ssl, byte[] sid_ctx, uint sid_ctx_len);

        [DllImport(SSLDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public extern static int SSL_do_handshake(IntPtr ssl);

        [DllImport(SSLDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public extern static void SSL_set_connect_state(IntPtr ssl);

        [DllImport(SSLDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public extern static void SSL_set_accept_state(IntPtr ssl);

        [DllImport(SSLDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public extern static int SSL_connect(IntPtr ssl);

        [DllImport(SSLDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public extern static IntPtr SSL_new(IntPtr ctx);

        [DllImport(SSLDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public extern static void SSL_free(IntPtr ssl);

        [DllImport(SSLDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public extern static int SSL_state(IntPtr ssl);

        [DllImport(SSLDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public extern static void SSL_set_state(IntPtr ssl, int state);

        [DllImport(SSLDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public extern static void SSL_set_bio(IntPtr ssl, IntPtr read_bio, IntPtr write_bio);

        [DllImport(SSLDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public extern static int SSL_use_certificate_file(IntPtr ssl, string file, int type);

        [DllImport(SSLDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public extern static int SSL_use_PrivateKey_file(IntPtr ssl, string file, int type);

        [DllImport(SSLDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public extern static int SSL_clear(IntPtr ssl);

        [DllImport(SSLDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public extern static IntPtr SSL_load_client_CA_file(string file);

        [DllImport(SSLDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public extern static IntPtr SSL_get_client_CA_list(IntPtr ssl);

        [DllImport(SSLDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public extern static void SSL_set_client_CA_list(IntPtr ssl, IntPtr name_list);

        [DllImport(SSLDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public extern static IntPtr SSL_get_certificate(IntPtr ssl);

        [DllImport(SSLDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public extern static int SSL_use_certificate(IntPtr ssl, IntPtr x509);

        [DllImport(SSLDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public extern static int SSL_use_PrivateKey(IntPtr ssl, IntPtr evp_pkey);

        [DllImport(SSLDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern int SSL_ctrl(IntPtr ssl, int cmd, int larg, IntPtr parg);

        [DllImport(SSLDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr SSL_get_servername(IntPtr s, int type);

        [DllImport(SSLDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern int SSL_get_servername_type(IntPtr s);

        [DllImport(SSLDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr SSL_get_session(IntPtr s);

        [DllImport(SSLDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern int SSL_CTX_callback_ctrl(IntPtr ctx, int cmd, IntPtr cb);

        #endregion
    }
}
