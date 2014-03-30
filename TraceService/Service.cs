using System;
using System.ServiceModel.Channels;
using System.Net;
using System.Collections.ObjectModel;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Runtime.Remoting.Messaging;

namespace TraceService
{
	/// <summary>
	/// Service.
	/// </summary>
	/// <remarks>
	/// Should be able to (eventually) - completely abstract the communication between Service and ServiceProxy
	/// (Is this what is called IoC or not??) - Currently MethodCall is basically doing this - generalise this class into a new one
	/// </remarks>
	public abstract class Service : IDisposable
	{
		public readonly Uri Uri;

		public readonly IRemotingFormatter Formatter;

		public readonly ConcurrentBag<TcpClient> Clients;

		protected Thread ServiceThread;

		protected bool ExitServiceThread = false;

		public bool IsRunning { get { return ServiceThread != null; }}

		/// <summary>
		/// Initializes a new instance of the <see cref="TraceService.Service"/> class.
		/// </summary>
		/// <param name="uri">URI.</param>
		public Service(Uri uri, IRemotingFormatter formatter,  bool start = true)
		{
			Uri = uri;
			Formatter = formatter;
			Clients = new ConcurrentBag<TcpClient>();
			if (start)
				Start();
		}

		/// <summary>
		/// Releases all resource used by the <see cref="TraceService.Service"/> object.
		/// </summary>
		/// <remarks>
		/// IDisposable implementation
		/// Call <see cref="Dispose"/> when you are finished using the <see cref="TraceService.Service"/>. The
		/// <see cref="Dispose"/> method leaves the <see cref="TraceService.Service"/> in an unusable state. After calling
		/// <see cref="Dispose"/>, you must release all references to the <see cref="TraceService.Service"/> so the garbage
		/// collector can reclaim the memory that the <see cref="TraceService.Service"/> was occupying.
		/// </remarks>
		public virtual void Dispose()
		{
			if (ServiceThread != null)
			{
				if (ServiceThread.IsAlive)
				{
					Stop();
					ServiceThread.Join();
				}
				ServiceThread = null;
			}
		}

		/// <summary>
		/// Close this instance.
		/// </summary>
		public void Stop()
		{
			ExitServiceThread = true;
		}

		/// <summary>
		/// Start this instance.
		/// </summary>
		public void Start()
		{
			ServiceThread = new Thread((thisService) => ((Service)thisService).RunService());
			ServiceThread.Start(this);
		}

		/// <summary>
		/// Runs the service.
		/// </summary>
		public void RunService()
		{

			while (!ExitServiceThread)
			{
				foreach (TcpClient client in Clients)
				{
					if (client != null)
					{
						NetworkStream clientStream = client.GetStream();
						if (clientStream.DataAvailable)
						{
							int size = client.Available;
							MethodCall.InvokeMethod(Formatter, clientStream, this);
							size = client.Available;
							;

						}
					}
				}
			}
			ServiceThread = null;
		}
	}
}

