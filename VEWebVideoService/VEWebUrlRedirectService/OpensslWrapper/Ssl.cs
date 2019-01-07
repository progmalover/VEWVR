
using System;
using System.Runtime.InteropServices;
using System.Text;
/*
 * Notice
 * This code is designed for vesystem,all right reserved. --maxwell.z
 * 2018.12
 */
namespace VEWebUrlRedirectService.OpensslWrapper
{
	internal enum SslError
	{
		SSL_ERROR_NONE = 0,
		SSL_ERROR_SSL = 1,
		SSL_ERROR_WANT_READ = 2,
		SSL_ERROR_WANT_WRITE = 3,
		SSL_ERROR_WANT_X509_LOOKUP = 4,
		SSL_ERROR_SYSCALL = 5,
		SSL_ERROR_ZERO_RETURN = 6,
		SSL_ERROR_WANT_CONNECT = 7,
		SSL_ERROR_WANT_ACCEPT = 8
	}

	/// <summary>
	/// Ssl.
	/// </summary>
	public class Ssl : Base
	{
		internal const int SSL_ST_CONNECT = 0x1000;
		internal const int SSL_ST_ACCEPT = 0x2000;

		#region Initialization

		/// <summary>
		/// Calls SSL_new()
		/// </summary>
		/// <param name="ctx"></param>
		internal Ssl(SslContext ctx)
		{
            base.Construct(Native.SSL_new(ctx.Handle),true);
        }

		internal Ssl(IntPtr ptr, bool takeOwnership) : base(ptr, takeOwnership)
		{
		}

		#endregion

		#region Properties

		internal int State
		{
			get { return Native.SSL_state(Handle); }
			set { Native.SSL_set_state(Handle, value); }
		}

		#endregion

		#region Methods

		internal int Accept()
		{
			return Native.SSL_accept(ptr);
		}
 

		internal SslError GetError(int ret_code)
		{
			return (SslError)Native.SSL_get_error(ptr, ret_code);
		}


		internal int Shutdown()
		{
			return Native.SSL_shutdown(ptr);
		}

		internal int Write(byte[] buf, int len)
		{
			return Native.SSL_write(ptr, buf, len);
		}

		internal int Read(byte[] buf, int len)
		{
			return Native.SSL_read(ptr, buf, len);
		}

        internal int SetFd(IntPtr fd)
        {
            return Native.SSL_set_fd(ptr,fd);
        }
		internal int SetSessionIdContext(byte[] sid_ctx, uint sid_ctx_len)
		{
			return Native.SSL_set_session_id_context(ptr, sid_ctx, sid_ctx_len);
		}

		internal int Renegotiate()
		{
			return Native.SSL_renegotiate(ptr);
		}

		internal int DoHandshake()
		{
			return Native.SSL_do_handshake(ptr);
		}

		internal void SetAcceptState()
		{
			Native.SSL_set_accept_state(ptr);
		}

		internal void SetConnectState()
		{
			Native.SSL_set_connect_state(ptr);
		}

	  

		internal int Clear()
		{
			return  Native.SSL_clear(ptr);
		}

        #endregion
        #region Overrides

        /// <summary>
        /// Calls SSL_free()
        /// </summary>
        protected override void OnDispose()
		{
			Native.SSL_free(Handle);
		}

		#endregion

	}
}
