using System;

namespace TraceService
{
	/// <summary>
	/// Listener.
	/// </summary>
	public abstract class Listener : Disposable
	{
		internal object SyncRoot = new object();

		/// <summary>
		/// Initializes a new instance of the <see cref="TraceService.Listener"/> class.
		/// </summary>
		public Listener()
		{
		}

		/// <summary>
		/// Releases unmanaged resources and performs other cleanup operations before the <see cref="TraceService.Listener"/>
		/// is reclaimed by garbage collection.
		/// </summary>
		~Listener()
		{
			Dispose(false);
		}

		/// <summary>
		/// Close this instance.
		/// </summary>
		public virtual void Close()
		{
			Dispose();
		}

		/// <summary>
		/// Trace the specified message.
		/// </summary>
		/// <param name="message">Message.</param>
		public abstract void Trace(Message message);
	}
}

