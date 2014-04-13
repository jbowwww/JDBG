using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Configuration;
using System.ServiceModel.Channels;

using System.Linq.Expressions;
using System.Threading;
using System.Net.Mime;
using System.Reflection;
using System.Diagnostics;


namespace TraceService
{
	public static class MainClass
	{
		public static ServiceHost Host;

		public static TimeSpan HostOpenCloseTimeout = TimeSpan.FromSeconds(10);

		public static int HostThreadLoopDelay = 222;

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
			AppDomain.CurrentDomain.UnhandledException += (object sender, UnhandledExceptionEventArgs e) =>
			{
				Exception ex = ((Exception)e.ExceptionObject);
				Console.WriteLine("Domain: {0}: {1}: Unhandled Exception: {2}\n{3}", DateTime.Now, sender ?? "(null)", ex.GetType().FullName, ex);
			};
			AppDomain.CurrentDomain.AssemblyLoad += (object sender, AssemblyLoadEventArgs args) =>
			{
				Console.WriteLine("Domain: {0}: {1}: Load Assembly: {2}", DateTime.Now, sender, args.LoadedAssembly.GetName());
			};
			AppDomain.CurrentDomain.DomainUnload += (object sender, EventArgs e) =>
			{
				Console.WriteLine("Domain: {0}: {1}: Domain Unload", DateTime.Now, sender ?? "(null)");
			};
			AppDomain.CurrentDomain.ProcessExit += (object sender, EventArgs e) =>
			{
				Console.WriteLine("Domain: {0}: {1}: Process Exit", DateTime.Now, sender ?? "(null)");
			};

			ProcessArgs(argv);
//			AppDomain.CurrentDomain.AssemblyResolve += (sender, args) => Assembly.LoadFile(args.);
			using (Host = new ServiceHost(typeof(TraceService)))
			{
				Host.OpenTimeout = Host.CloseTimeout = Timeout;
				ServiceEndpoint epTraceService = Host.AddServiceEndpoint(typeof(ITraceService), Binding, BaseUri);

				foreach (OperationDescription op in epTraceService.Contract.Operations)
				{
					DataContractSerializerOperationBehavior dcsob = op.Behaviors.Find<DataContractSerializerOperationBehavior>();
					if (dcsob == null)
						op.Behaviors.Add(dcsob = new DataContractSerializerOperationBehavior(op, new DataContractFormatAttribute() { }));
					dcsob.DataContractSurrogate = new TraceServiceSurrogate();			//			dcsob.DataContractResolver = new System.Runtime.Serialization.DataContractResolver
				}

				Host.Opening += (sender, e) => {
					Console.WriteLine("Service: {0}: {1}: Opening: {2}{3}: {4}", DateTime.Now, sender.ToString(),
						Host.Description.Namespace, Host.Description.Name, Host.Description.ServiceType.FullName);
					foreach (Uri baseUri in Host.BaseAddresses)
						Console.WriteLine("    Base Address: {0}", Host.BaseAddresses[0]);
					foreach (ServiceEndpoint endpoint in Host.Description.Endpoints)
						Console.WriteLine("    Endpoint: {0}{1} {2}", endpoint.Contract.Namespace, endpoint.Contract.Name, endpoint.Address.Uri.ToString());
				};
				Host.Opened += (sender, e) => Console.WriteLine("Service: {0}: {1}: Opened", DateTime.Now, sender.ToString());
				Host.Closing += (sender, e) => Console.WriteLine("Service: {0}: {1}: Closing", DateTime.Now, sender.ToString());
				Host.Closed += (sender, e) => Console.WriteLine("Service: {0}: {1}: Closed", DateTime.Now, sender.ToString());
				Host.UnknownMessageReceived += (sender, e) => Console.WriteLine("Service: {0}: {1}: UnknownMessageReceived: {2}", DateTime.Now, sender.ToString(), e.Message.ToString());

				Host.Open();
//				while (Host.State == CommunicationState.Opening || Host.State == CommunicationState.Opened)
//				{
					Thread.Sleep(HostThreadLoopDelay);
//				}
				Process.GetCurrentProcess().WaitForExit();
			}
		}
	}
}
