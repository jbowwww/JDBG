using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Remoting.Messaging;
using System.ServiceModel.Channels;
using System.Net.Sockets;

namespace TraceService
{
	public abstract class ServiceProxy
	{
		public readonly Uri Uri;

		public readonly IRemotingFormatter Formatter;

		protected readonly TcpClient Service;

		protected readonly NetworkStream ServiceStream;

		public ServiceProxy(Uri uri, IRemotingFormatter formatter)
		{
			Uri = uri;
			Formatter = formatter;
			Service = new TcpClient(uri.Host, uri.Port);

			ServiceStream = Service.GetStream();
		}

		public void Invoke(MethodCall methodCall)
		{
			methodCall.InvokeFromProxy(Formatter, ServiceStream);
			int size = Service.Available;
		}

	}
}

