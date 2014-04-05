using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Configuration;
using System.ServiceModel.Channels;

using System.Linq.Expressions;
using System.Threading;
using System.Net.Mime;


namespace TraceService
{
	public static class MainClass
	{
		public static ServiceHost Host;

		public static TimeSpan HostOpenCloseTimeout = TimeSpan.FromSeconds(10);

		public static int HostThreadLoopDelay = 111;

		public static TraceService Service;

		public static string HostName = "localhost";

		public static int Port = 7777;

		public static Binding Binding = new NetTcpBinding();

		public static string BaseUri {
			get { return string.Format("{0}://{1}:{2}/{3}/", Binding.Scheme, HostName, Port, typeof(ITraceService).Name); }
		}

		public static TimeSpan Timeout = new TimeSpan(0, 0, 10);

		private static void ProcessArgs(string[] argv)
		{
//			Environment.
			foreach (string arg in argv)
			{
				if (arg.StartsWith("--hostname=") && arg.Length > 11)
					HostName = arg.Substring(11);
				else if (arg.StartsWith("--port=") && arg.Length > 7)
					Port = int.Parse(arg.Substring(7));
			}
		}

		public static void Main(string[] argv)
		{
			ProcessArgs(argv);

			using (Host = new ServiceHost(typeof(TraceService)))		//, new Uri[] { new Uri(BaseUri) }))
			{
				Host.OpenTimeout = Host.CloseTimeout = Timeout;
				Host.AddServiceEndpoint(typeof(ITraceService), Binding, BaseUri);

				Host.Opening += (sender, e) => Console.WriteLine("Service: {0}: Opening", sender.ToString());		// += async (sender, e) => Console.WriteLine("{0}: Opening", sender.ToString());
				Host.Opened += (sender, e) => Console.WriteLine("Service: {0}: Opened", sender.ToString());
				Host.Closing += (sender, e) => Console.WriteLine("Service: {0}: Closing", sender.ToString());
				Host.Closed += (sender, e) => Console.WriteLine("Service: {0}: Closed", sender.ToString());
				Host.UnknownMessageReceived += (sender, e) => Console.WriteLine("Service: {0}: UnknownMessageReceived: {1}", sender.ToString(), e.Message.ToString());

				Console.WriteLine("Service: Host: {0}{1} - {2}", Host.Description.Namespace, Host.Description.Name, Host.Description.ServiceType.FullName);
				if (Host.BaseAddresses.Count > 0)
				{
					Console.WriteLine("Service: Base Addresses:");
					foreach (Uri baseUri in Host.BaseAddresses)
						Console.WriteLine("\t{0}", baseUri.ToString());
				}
				if (Host.Description.Endpoints.Count > 0)
				{
					Console.WriteLine("Service: Endpoints:");
					foreach (ServiceEndpoint endpoint in Host.Description.Endpoints)
						Console.WriteLine("\t{0}{1} {2}", endpoint.Contract.Namespace, endpoint.Contract.Name, endpoint.Address.Uri.ToString());
				}
				Console.WriteLine();

				Host.Open();
				while (Host.State == CommunicationState.Opening || Host.State == CommunicationState.Opened)
				{
					Thread.Sleep(HostThreadLoopDelay);
				}
			}
		}
	}
}
