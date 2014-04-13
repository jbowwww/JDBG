using System;
using System.ServiceModel;
using System.Runtime.Remoting.Messaging;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.ServiceModel.Channels;
using System.Diagnostics;
using System.Security.AccessControl;
using System.ServiceModel.Description;
using System.Runtime.Serialization;
using System.Reflection;

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

//		[ThreadStatic]
		ChannelFactory<ITraceService> _factory;

//		[ThreadStatic]
		private ITraceService _channel = null;

		private IClientChannel _clientChannel = null;

		protected ITraceService Channel {
			get
			{
				lock (_proxyLock)
				{
					if (_factory == null)
					{
						_factory = new ChannelFactory<ITraceService>(Binding, Endpoint);
						foreach (OperationDescription op in _factory.Endpoint.Contract.Operations)
						{
							DataContractSerializerOperationBehavior dcsob = op.Behaviors.Find<DataContractSerializerOperationBehavior>();
							if (dcsob == null)
								op.Behaviors.Add(dcsob = new DataContractSerializerOperationBehavior(op, new DataContractFormatAttribute() { }));
							dcsob.DataContractSurrogate = new TraceServiceSurrogate();			//		dcsob.DataContractResolver = new System.Runtime.Serialization.DataContractResolver
						}
						_factory.Opening += (sender, e) => Console.WriteLine("Client: {0}: Factory: Opening: {1} - {2}", DateTime.Now, sender.ToString(), _factory.State.ToString());
						_factory.Opened += (sender, e) => Console.WriteLine("Client: {0}: Factory: Opened: {1} - {2}", DateTime.Now, sender.ToString(), _factory.State.ToString());
						_factory.Closing += (sender, e) => Console.WriteLine("Client: {0}: Factory: Closing: {1} - {2}", DateTime.Now, sender.ToString(), _factory.State.ToString());
						_factory.Closed += (sender, e) => { Console.WriteLine("Client: {0}: Factory: Closed: {1} - {2}", DateTime.Now, sender.ToString(), _factory.State.ToString()); _factory = null; };
						_factory.Faulted += (sender, e) => Console.WriteLine("Client: {0}: Factory: Faulted: {1} - {2}", DateTime.Now, sender.ToString(), _factory.State.ToString());
					}
					if (_channel == null)
					{
						_channel = _factory.CreateChannel();
						_clientChannel = ((IClientChannel)_channel);
						_clientChannel.Opening += (sender, e) => Console.WriteLine("Client: {0}: Channel: Opening: {1} - {2}", DateTime.Now, sender.ToString(), _clientChannel.State.ToString());
						_clientChannel.Opened += (sender, e) => Console.WriteLine("Client: {0}: Channel: Opened: {1} - {2}", DateTime.Now, sender.ToString(), _clientChannel.State.ToString());
						_clientChannel.Closing += (sender, e) => Console.WriteLine("Client: {0}: Channel: Closing: {1} - {2}", DateTime.Now, sender.ToString(), _clientChannel.State.ToString());
						_clientChannel.Closed += (sender, e) => { Console.WriteLine("Client: {0}: Channel: Closed: {1} - {2}", DateTime.Now, sender.ToString(), _clientChannel.State.ToString()); _channel = null; };
						_clientChannel.Faulted += (sender, e) => Console.WriteLine("Client: {0}: Channel: Faulted: {1} - {2}", DateTime.Now, sender.ToString(), _clientChannel.State.ToString());
						_clientChannel.UnknownMessageReceived += (sender, e) => Console.WriteLine("Client: {0}: Channel: UnknownMessageReceived: {1} - {2}", DateTime.Now, sender.ToString(), _clientChannel.State.ToString());
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
			LoadAssembly(Assembly.GetEntryAssembly().Location);
		}

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
					_clientChannel.Close();
				if (_factory != null)
					_factory.Close();
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
		/// Loads the assembly.
		/// </summary>
		/// <param name="path">Path.</param>
		public void LoadAssembly(string path)
		{
			Channel.LoadAssembly(path);
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

