using System;
using System.ServiceModel;
using System.Runtime.Remoting.Messaging;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.ServiceModel.Channels;
using System.Diagnostics;
using System.Security.AccessControl;

namespace TraceService
{
	/// <summary>
	/// Trace service proxy.
	/// </summary>
	[ServiceBehavior(IncludeExceptionDetailInFaults = true)]
	public class TraceProxy : Disposable, ITraceService
	{
		Binding Binding;
		EndpointAddress Endpoint;

		private readonly object _proxyLock = new object();

		[ThreadStatic]
		ChannelFactory<ITraceService> _factory;

		[ThreadStatic]
		private ITraceService _channel = null;
//		private readonly TraceService _channel;

		protected ITraceService Channel {
			get
			{
				lock (_proxyLock)
				{
					if (_factory == null)
					{
						_factory = new ChannelFactory<ITraceService>(Binding, Endpoint);
						Console.WriteLine("Client: Created factory {0} - {1}", _factory.ToString(), _factory.State.ToString());
					}
					if (_factory.State == CommunicationState.Created)
					{
						_factory.Open();
						Console.WriteLine("Client: Opening factory {0} - {1}", _factory.ToString(), _factory.State.ToString());
					}
					if (_channel == null)
					{
						_channel = _factory.CreateChannel();
						Console.WriteLine("Client: Created proxy channel {0} - {1}", _channel.ToString(), ((ICommunicationObject)_channel).State.ToString());
					}
					if (((ICommunicationObject)_channel).State == CommunicationState.Created)
					{
						((ICommunicationObject)_channel).Open();
						Console.WriteLine("Client: Opening channel {0} - {1}", _channel.ToString(), ((ICommunicationObject)_channel).State.ToString());
					}
					return _channel;
				}
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="TraceService.TraceServiceProxy"/> class.
		/// </summary>
		public TraceProxy(Binding binding, EndpointAddress endpoint)
		{
			Binding = binding;
			Endpoint = endpoint;
		}
//			_channel = ChannelFactory<ITraceService>.CreateChannel(binding, endpoint);
//			using (ChannelFactory<ITraceService> _factory = new ChannelFactory<ITraceService>(binding, endpoint))
//			{
//				_factory.Open();
//				_channel = _factory.CreateChannel();
//			}

		/// <summary>
		/// Releases unmanaged resources and performs other cleanup operations before the
		/// <see cref="TraceService.TraceProxy"/> is reclaimed by garbage collection.
		/// </summary>
		~TraceProxy()
		{
			Dispose(false);
		}

		/// <summary>
		/// Disposes the managed resources
		/// </summary>
		protected override void DisposeManaged()
		{
			lock (_proxyLock)
			{
				if (_channel != null)
				{
					ICommunicationObject commObj = (ICommunicationObject)_channel;
					if (commObj.State == CommunicationState.Opened)
					{
						commObj.Close();
						Console.WriteLine("Client: Closed channel {0} - {1}", _channel.ToString(), commObj.State.ToString());
					}
					_channel = null;
				}
				if (_factory != null)
				{
					if (_factory.State == CommunicationState.Opened)
					{
						_factory.Close();
						Console.WriteLine("Client: Closed factory {0} - {1}", _factory.ToString(), _factory.State.ToString());
					}
					_factory = null;
//				((IDisposable)_factory).Dispose();
				}
			}
		}

		/// <summary>
		/// Close this instance.
		/// </summary>
		public void Close()
		{
			Dispose();
		}

		/// <summary>
		/// Trace the specified message.
		/// </summary>
		/// <param name="message">Message.</param>
		/// <remarks>ITraceService implementation</remarks>
		public void Trace(Message message)
		{
			Channel.Trace(message);
		}
	}
}

