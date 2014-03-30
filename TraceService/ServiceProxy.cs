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

		protected readonly NetworkStream ServiceStream;

		public ServiceProxy(Uri uri, IRemotingFormatter formatter)
		{
			Uri = uri;
			Formatter = formatter;
			TcpClient service = new TcpClient(uri.Host, uri.Port);
			ServiceStream = service.GetStream();
		}

		public void Invoke(ServiceMethodCall methodCall)
		{
			methodCall.InvokeFromProxy(Formatter, ServiceStream);

		}

	}
}

