using System;

namespace TraceService
{
	public class TraceProxyListener : Listener
	{
		public TraceProxy Proxy { get; private set; }

		public TraceProxyListener(TraceProxy proxy)
		{
			Proxy = proxy;
		}

		/// <summary>
		/// Trace the specified message.
		/// </summary>
		/// <param name="message">Message.</param>
		/// <remarks>Listener implementation</remarks>
		public override void Trace(Message message)
		{
			Proxy.Trace(message);
		}
	}
}

