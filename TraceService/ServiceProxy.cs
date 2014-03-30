using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Remoting.Messaging;
using System.ServiceModel.Channels;
using System.Net.Sockets;

namespace TraceService
{
	public abstract class ServiceProxy : IDisposable
	{
		public readonly Uri Uri;

		public readonly IRemotingFormatter Formatter;

		protected TcpClient Service { get; private set; }

		protected NetworkStream ServiceStream { get; private set; }

		public ServiceProxy(Uri uri, IRemotingFormatter formatter)
		{
			Uri = uri;
			Formatter = formatter;
			Service = new TcpClient(uri.Host, uri.Port);

			ServiceStream = Service.GetStream();
		}

		/// <summary>
		/// Releases all resource used by the <see cref="TraceService.ServiceProxy"/> object.
		/// </summary>
		/// <remarks>
		/// Call <see cref="Dispose"/> when you are finished using the <see cref="TraceService.ServiceProxy"/>. The
		/// <see cref="Dispose"/> method leaves the <see cref="TraceService.ServiceProxy"/> in an unusable state. After
		/// calling <see cref="Dispose"/>, you must release all references to the <see cref="TraceService.ServiceProxy"/> so
		/// the garbage collector can reclaim the memory that the <see cref="TraceService.ServiceProxy"/> was occupying.
		/// IDisposable implementation
		/// </remarks>
		public void Dispose()
		{
			if (Service != null)
			{
				Close();
				Service = null;
			}

		}

		public void Flush()
		{
			ServiceStream.Flush();
		}

		public void Close()
		{
			Service.Close();
			ServiceStream = null;
		}

		public void Invoke(MethodCall methodCall)
		{
			methodCall.InvokeFromProxy(Formatter, ServiceStream);
			int size = Service.Available;
		}

	}
}

