using System;

namespace TraceService
{
	/// <summary>
	/// Listener.
	/// </summary>
	public abstract class Listener
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="TraceService.Listener"/> class.
		/// </summary>
		public Listener()
		{
		}

		/// <summary>
		/// Close this instance.
		/// </summary>
		public abstract void Close();

		/// <summary>
		/// Trace the specified message.
		/// </summary>
		/// <param name="message">Message.</param>
		public abstract void Trace(Message message);
	}
}

