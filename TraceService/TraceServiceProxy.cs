using System;
using System.ServiceModel;
using System.Runtime.Remoting.Messaging;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;

namespace TraceService
{
	/// <summary>
	/// Trace service proxy.
	/// </summary>
	[ServiceBehavior(IncludeExceptionDetailInFaults = true)]
	public class TraceServiceProxy : ServiceProxy, ITraceService
	{
		private ITraceService _service;

		TcpClient Client;

		NetworkStream ClientStream;

		/// <summary>
		/// Initializes a new instance of the <see cref="TraceService.TraceServiceProxy"/> class.
		/// </summary>
		public TraceServiceProxy(IRemotingFormatter formatter, string uri = "net.tcp://localhost:7777/Trace")
			: base(new Uri(uri), formatter)
		{
//			if (factory.State == CommunicationState.Created)
//				factory.Open(TimeSpan.FromSeconds(10));
			//_service = ChannelFactory<ITraceService>.CreateChannel(new NetTcpBinding(), new EndpointAddress(uri));
//			_service = factory.CreateChannel(new EndpointAddress(uri));
			Client = new TcpClient("localhost", 7777);
			ClientStream = Client.GetStream();
		}

		/// <summary>
		/// Trace the specified message.
		/// </summary>
		/// <param name="message">Message.</param>
		/// <remarks>ITraceService implementation</remarks>
		public void Trace(TraceMessage message)
		{
//			Formatter.Serialize(ClientStream, message);
			base.Invoke(new ServiceMethodCall(typeof(ITraceService), "Trace", message));
		}

		/// <summary>
		/// Exits the trace service.
		/// </summary>
		/// <remarks>ITraceService implementation</remarks>
		public void ExitTraceService()
		{
//			_service.ExitTraceService();
			base.Invoke(new ServiceMethodCall(typeof(ITraceService), "ExitTraceService"));
		}
	}
}

