 

using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace VEWebUrlRedirectService.OpensslWrapper
{
 /*
 * Notice
 * The origin version of this file get from openssl.net project on github.
 * i modified it according to project requirement. --maxwell.z
 * 2018.12
 */
    /// <summary>
    /// Base class for all openssl wrapped objects. 
    /// Contains the raw unmanaged pointer and has a Handle property to get access to it. 
    /// Also overloads the ToString() method with a BIO print.
    /// </summary>
    public abstract class Base : IDisposable
	{
		/// <summary>
		/// Constructor which takes the raw unmanaged pointer. 
		/// This is the only way to construct this object and all derived types.
		/// </summary>
		/// <param name="ptr"></param>
		/// <param name="takeOwnership"></param>
		protected Base(IntPtr ptr, bool takeOwnership)
		{
            Construct(ptr, takeOwnership);
        }

        protected Base()
        {
        }

        public void Construct(IntPtr ptr, bool takeOwnership)
        {
            this.ptr = ptr;
            owner = takeOwnership;
            if (this.ptr != IntPtr.Zero)
            {
                OnNewHandle(this.ptr);
            }
        }
        /// <summary>
        /// This finalizer just calls Dispose().
        /// </summary>
        ~Base()
		{
			Dispose();
		}


		/// <summary>
		/// This method must be implemented in derived classes.
		/// </summary>
		protected abstract void OnDispose();

		/// <summary>
		/// Do nothing in the base class.
		/// </summary>
		/// <param name="ptr"></param>
		internal virtual void OnNewHandle(IntPtr ptr)
		{
		}

		#region IDisposable Members

		/// <summary>
		/// Implementation of the IDisposable interface.
		/// If the native pointer is not null, we haven't been disposed, and we are the owner,
		/// then call the virtual OnDispose() method.
		/// </summary>
		public void Dispose()
		{
			if (!isDisposed && owner && ptr != IntPtr.Zero)
			{
				OnDispose();
				DoAfterDispose();
			}

			isDisposed = true;
		}

		#endregion

		/// <summary>
		/// gets/sets whether the object owns the Native pointer
		/// </summary>
		public virtual bool IsOwner
		{
			get { return owner; }
			internal set { owner = value; }
		}

		/// <summary>
		/// Access to the raw unmanaged pointer.
		/// </summary>
		public virtual IntPtr Handle
		{
			get { return ptr; }
		}

		private void DoAfterDispose()
		{
			ptr = IntPtr.Zero;
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Raw unmanaged pointer
		/// </summary>
		protected IntPtr ptr;

		/// <summary>
		/// If this object is the owner, then call the appropriate native free function.
		/// </summary>
		protected bool owner = false;

		/// <summary>
		/// This is to prevent double-deletion issues.
		/// </summary>
		protected bool isDisposed = false;
	}

	/// <summary>
	/// Helper type that handles the AddRef() method.
	/// </summary>
	public abstract class BaseReference : Base
	{
		internal BaseReference(IntPtr ptr, bool takeOwnership)
			: base(ptr, takeOwnership)
		{
		}

		internal abstract void AddRef();
	}

	/// <summary>
	/// Derived classes must implement the <code>LockType</code> and <code>RawReferenceType</code> properties
	/// </summary>
	public abstract class BaseReferenceImpl : BaseReference
	{
		internal BaseReferenceImpl(IntPtr ptr, bool takeOwnership)
			: base(ptr, takeOwnership)
		{
			var offset = Marshal.OffsetOf(RawReferenceType, "references");
			refPtr = new IntPtr((long)ptr + (long)offset);
		}

		/// <summary>
		/// Prints the current underlying reference count 
		/// </summary>
		public void PrintRefCount()
		{
			var count = Marshal.ReadInt32(refPtr);
			Console.WriteLine("{0} ptr: {1}, ref_count: {2}", 
				this.GetType().Name, this.ptr, count
			);
		}

		/// <summary>
		/// Gets the reference count.
		/// </summary>
		/// <value>The reference count.</value>
		public int RefCount
		{
			get { return Marshal.ReadInt32(refPtr); }
		}
		 
		internal abstract Type RawReferenceType { get; }

		private IntPtr refPtr;
	}

	/// <summary>
	/// Helper base class that handles the AddRef() method by using a _dup() method.
	/// </summary>
	public abstract class BaseValue : BaseReference
	{
		internal BaseValue(IntPtr ptr, bool takeOwnership)
			: base(ptr, takeOwnership)
		{
		}

		internal override void AddRef()
		{
			ptr = DuplicateHandle();
			owner = true;

			if (ptr != IntPtr.Zero)
			{
				OnNewHandle(ptr);
			}
		}

		/// <summary>
		/// Derived classes must use a _dup() method to make a copy of the underlying native data structure.
		/// </summary>
		/// <returns></returns>
		internal abstract IntPtr DuplicateHandle();
	}
}
