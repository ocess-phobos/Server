using System;

namespace MiscUtil.Core.Threading
{
	/// <summary>
	/// A lock token returned by a Lock method call on a SyncLock.
	/// This effectively holds the lock until it is disposed - a 
	/// slight violation of the IDisposable contract, but it makes
	/// for easy use of the SyncLock system. This type itself
	/// is not thread-safe - LockTokens should not be shared between
	/// threads.
	/// </summary>
	public struct LockToken : IDisposable
	{
		/// <summary>
		/// The lock this token has been created by.
		/// </summary>
		SyncLock parent;

		/// <summary>
		/// Constructs a new lock token for the specified lock.
		/// </summary>
		/// <param name="parent">The internal monitor used for locking.</param>
		internal LockToken (SyncLock parent)
		{
			this.parent = parent;
		}

		/// <summary>
		/// Releases the lock. Subsequent calls to this method do nothing.
		/// </summary>
		public void Dispose()
		{
			if (this.parent==null)
			{
				return;
			}
			this.parent.Unlock();
			this.parent = null;
		}
	}
}
