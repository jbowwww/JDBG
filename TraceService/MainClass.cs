using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Channels;
using System.ServiceModel;
using System.Net.Mime;
using System.Net.Sockets;
using System.Net;
using System.ServiceModel.Description;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;

namespace TraceService
{
	public static class MainClass
	{
		public static TimeSpan HostOpenCloseTimeout = TimeSpan.FromSeconds(10);

		public static ServiceHost Host;

		public static TraceService Service;

		public static Server<ITraceService> Server;

		public static string HostName = "localhost";

		public static int Port = 7777;

		public static TcpListener Listener;

		public static TcpClient Client;

		public static NetworkStream ClientStream;

		public static BinaryFormatter Formatter;

		public static bool ExitService = false;

		public static void Main(string[] argv)
		{
			foreach (string arg in argv)
			{
				if (arg.StartsWith("--hostname=") && arg.Length > 11)
					HostName = arg.Substring(11);
				else if (arg.StartsWith("--port=") && arg.Length > 7)
					Port = int.Parse(arg.Substring(7));
			}

			using (Service = new TraceService())
			{
				using (Server = new Server<ITraceService>(Service))
				{
					Console.WriteLine("Started server for service {0} at {1} using {2}", Service.GetType().Name, Service.Uri.ToString(), Service.Formatter.GetType().Name);
					while (Server.IsRunning || Service.IsRunning)
						Thread.Sleep(50);
					Console.WriteLine("Closing service host...");
				}
			}
		}
	}
}

//		public static void Main(string[] argv)
//		{
//			foreach (string arg in argv)
//			{
//				if (arg.StartsWith("--hostname=") && arg.Length > 11)
//					HostName = arg.Substring(11);
//				else if (arg.StartsWith("--port=") && arg.Length > 7)
//					Port = int.Parse(arg.Substring(7));
//			}
//
//			using (Service = new TraceService())
//			{
//				TraceMessage message;
//				Formatter = new BinaryFormatter();
//				Listener = new TcpListener(IPAddress.Any, 7777);
//				Listener.Start();
//				Console.WriteLine("Started service host for {0}: {1}", Service.GetType().Name, Listener.ToString());
//
//	//			List<byte> buf = new List<byte>();
//	//			byte[] buf2;
//				Client = Listener.AcceptTcpClient();
//				ClientStream = Client.GetStream();
//
//				while (!ExitService)
//				{
//	//				buf.Clear();
//					while (ClientStream.DataAvailable)
//					{
//	//					buf.Add(Convert.ToByte(ClientStream.ReadByte()));
//	//					if (buf.GetRange(buf.Count - 4, 4).ToArray().Equals(new byte[] { 255, 128, 64, 32 }))
//	//						buf.RemoveRange(buf.Count - 4, 4);
//						message = (TraceMessage)Formatter.Deserialize(ClientStream);
//						Service.Trace(message);
//					}
//				}
//
//				Console.WriteLine("Closing service host...");
//				ClientStream.Close();
//				Client.Close();
//				Listener.Stop();
//			}
//		}

//			Host = new ServiceHost(typeof(TraceService), new Uri(string.Format("net.tcp://{0}:{1}/", HostName, Port)));
//			Host.AddServiceEndpoint(typeof(ITraceService), new NetTcpBinding(), "Trace");
//			Host.Open(HostOpenCloseTimeout);
//			Console.WriteLine("Started service host for {0}{1}: {2}", Host.Description.Namespace, Host.Description.Name, Host.State.ToString());
//			while (!ExitService)
//			{
//				
//			}
//
//			Console.WriteLine("Closing service host...");
//			Host.Close(HostOpenCloseTimeout);
