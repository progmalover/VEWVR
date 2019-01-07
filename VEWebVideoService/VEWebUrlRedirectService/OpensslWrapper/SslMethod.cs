
/*
* Notice
* This code is designed for vesystem,all right reserved. --maxwell.z
* 2018.12
*/

using System;

namespace VEWebUrlRedirectService.OpensslWrapper
{
	/// <summary>
	/// Wraps the SSL_METHOD structure and methods
	/// </summary>
	public class SslMethod : Base
	{
		public SslMethod(IntPtr ptr, bool owner) :
			base(ptr, owner)
		{
		}

		/// <summary>
		/// Throws NotImplementedException()
		/// </summary>
		protected override void OnDispose()
		{
			//throw new NotImplementedException();
		}

 
	 
		/// <summary>
		/// SSLv23_server_method()
		/// </summary>
		public static SslMethod SSL_server_method = new SslMethod(Native.TLS_server_method(), false);
        public static SslMethod SSLv1_server_method = new SslMethod(Native.TLSv1_server_method(), false);
        public static SslMethod SSLv11_server_method = new SslMethod(Native.TLSv1_1_server_method(), false);
        public static SslMethod SSLv12_server_method = new SslMethod(Native.TLSv1_2_server_method(), false);
        /// <summary>



    }
}
