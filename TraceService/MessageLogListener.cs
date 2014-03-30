using System;
using System.Collections.Generic;

namespace TraceService
{
	/// <summary>
	/// Message log listener.
	/// </summary>
	public class MessageLogListener : Listener
	{
		/// <summary>
		/// The log.
		/// </summary>
		public readonly List<Message> Log;

		/// <summary>
		/// Initializes a new instance of the <see cref="TraceService.MessageLogListener"/> class.
		/// </summary>
		public MessageLogListener()
		{
			Log = new List<Message>();

		}

		/// <summary>
		/// Close this instance.
		/// </summary>
		/// <remarks>Listener implementation</remarks>
		public override void Close()
		{
		}

		/// <summary>
		/// Trace the specified message.
		/// </summary>
		/// <param name="message">Message.</param>
		/// <remarks>Listener implementation</remarks>
		public override void Trace(Message message)
		{
			Log.Add(message);
		}
	}
}

