using System;
using TraceService;
using System.Diagnostics;
using System.Threading;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Collections;

namespace TraceClientTest
{
	public static class MainClass
	{
		public readonly static Source Trace = Source.GetOrCreate("TraceClientTest", true, new ConsoleListener());

		public static Process TraceServiceProcess;

		public static string HostName = "localhost";

		public static int Port = 7777;

		public static Binding Binding = new NetTcpBinding();

		public static string BaseUri {
			get { return string.Format("{0}://{1}:{2}/{3}/", Binding.Scheme, HostName, Port, typeof(ITraceService).Name); }
		}

		public static TimeSpan Timeout = new TimeSpan(0, 0, 10);

		public static TraceProxy TestProxy;

		public static void Main(string[] argv)
		{
			Console.ReadLine();

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

			using (TraceServiceProcess = Process.Start("../../../TraceService/bin/Debug/TraceService.exe"))
			{		
				TraceServiceProcess.OutputDataReceived += (sender, e) => Console.WriteLine("Service: {0}: Process Output Data Received: {1}: {2}", DateTime.Now, sender, e.Data);
				TraceServiceProcess.ErrorDataReceived += (sender, e) => Console.WriteLine("Service: {0}: Process Error Data Received: {1}: {2}", DateTime.Now, sender, e.Data);
				TraceServiceProcess.Disposed += (sender, e) => Console.WriteLine("Service: {0}: Process Disposed", DateTime.Now);
				TraceServiceProcess.Exited += (sender, e) => Console.WriteLine("Service: {0}: Process Exited: {1}: {2}", DateTime.Now, TraceServiceProcess.ExitCode, TraceServiceProcess.ExitTime);

				Thread.Sleep(1100);
				using (TestProxy = new TraceProxy(Binding, new EndpointAddress(BaseUri)))
				{
					Trace.Listeners.Add(new TraceProxyListener(TestProxy));

					Trace.Log(MessageLevel.Information, "Test", "Testicles 1 2", new Guid());
					Trace.Log(MessageLevel.Verbose, "Test", "Bitses", new BitArray(32));

					Source.StopAll();
					Source.WaitAll();

					TestProxy.Close();
				}
			}
		}
	}
}

