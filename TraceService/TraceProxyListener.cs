using System;

namespace TraceService
{
	public class TraceProxyListener : Listener, IDisposable
	{
		public TraceProxy Proxy { get; private set; }

		public TraceProxyListener(TraceProxy proxy)
		{
			Proxy = proxy;
		}

		/// <summary>
		/// Releases all resource used by the <see cref="TraceService.ServiceProxyListener"/> object.
		/// </summary>
		/// <remarks>
		/// Call <see cref="Dispose"/> when you are finished using the <see cref="TraceService.ServiceProxyListener"/>. The
		/// <see cref="Dispose"/> method leaves the <see cref="TraceService.ServiceProxyListener"/> in an unusable state.
		/// After calling <see cref="Dispose"/>, you must release all references to the
		/// <see cref="TraceService.ServiceProxyListener"/> so the garbage collector can reclaim the memory that the
		/// <see cref="TraceService.ServiceProxyListener"/> was occupying.
		/// IDisposable implementation
		/// </remarks>
		public void Dispose()
		{
			if (Proxy != null)
			{
				Close();
				Proxy = null;
			}
		}

		/// <summary>
		/// Close this instance.
		/// </summary>
		/// <remarks>Listener implementation</remarks>
		public override void Close()
		{
			Proxy.Dispose();
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

