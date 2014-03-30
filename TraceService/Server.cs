using System;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Runtime.Remoting.Messaging;

namespace TraceService
{
	public class Server<TService> : IDisposable
	{
		public Uri Uri { get; protected set; }

		public IPHostEntry Host { get; protected set; }

		/// <summary>
		/// Currently only one service instance per server, all clients coonnect to same instance
		/// Make this configurable to multiple instances
		/// </summary>
		public Service Service { get; protected set; }

		protected TcpListener Listener;
	
		protected Thread ListenThread;

		protected bool ExitListenThread = false;

		public bool IsRunning { get { return ListenThread != null; }}

		/// <summary>
		/// Initializes a new instance of the <see cref="TraceService.Server`1"/> class.
		/// </summary>
		/// <param name="service">Service.</param>
		public Server(Service service, bool start = true)
		{
			Service = service;
			Uri = service.Uri;
			Host = new IPHostEntry() { HostName = Uri.Host };
			if (start)
				Start();
		}

		/// <summary>
		/// Releases all resource used by the <see cref="TraceService.Server`1"/> object.
		/// </summary>
		/// <remarks>
		/// IDisposable implementation
		/// Call <see cref="Dispose"/> when you are finished using the <see cref="TraceService.Server`1"/>. The
		/// <see cref="Dispose"/> method leaves the <see cref="TraceService.Server`1"/> in an unusable state. After calling
		/// <see cref="Dispose"/>, you must release all references to the <see cref="TraceService.Server`1"/> so the garbage
		/// collector can reclaim the memory that the <see cref="TraceService.Server`1"/> was occupying.
		/// </remarks>
		public void Dispose()
		{
			if (ListenThread != null)
			{
				if (ListenThread.IsAlive)
				{
					Stop();
					ListenThread.Join();
				}
				ListenThread = null;
			}
		}

		/// <summary>
		/// Close this instance.
		/// </summary>
		public void Stop()
		{
			ExitListenThread = true;
		}

		/// <summary>
		/// Start this instance.
		/// </summary>
		public void Start()
		{
			ListenThread = new Thread((thisServer) => { ((Server<TService>)thisServer).ServerListen(); });
			ListenThread.Start(this);
		}

		/// <summary>
		/// Servers the listen.
		/// </summary>
		protected void ServerListen()
		{
			Listener =  new TcpListener( /*Host.AddressList[0]*/ Dns.GetHostAddresses(Uri.Host)[0], Uri.Port);
			Listener.Start();

			while (!ExitListenThread)
			{
				while (Listener.Pending())
				{
					TcpClient client = Listener.AcceptTcpClient();
					Service.ClientStreams.Add(client.GetStream());
				}
				Thread.Yield();
			}
			Listener = null;
		}
	}
}

