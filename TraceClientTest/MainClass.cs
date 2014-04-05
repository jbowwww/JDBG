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
			using (TraceServiceProcess = Process.Start("../../../TraceService/bin/Debug/TraceService.exe"))
			{
				Console.ReadKey();
				Console.WriteLine("Client: Creating proxy...");
//			new Thread(() => { TraceService.MainClass.Main(new string[] { }); }).Start(); 
//				Thread.Sleep(1000);

				TestProxy = new TraceProxy(Binding, new EndpointAddress(BaseUri));
				Trace.Listeners.Add(new TraceProxyListener(TestProxy));

				Trace.Log(new TraceService.Message(new Guid()) { Level = MessageLevel.Information, Category = "Test", Description = "Testicles 1 2" });
				Trace.Log(new TraceService.Message(new BitArray(32)) { Level = MessageLevel.Verbose, Category = "Test", Description = "Bitses" });

//				Thread.Sleep(1800);

				Console.WriteLine("Client: Stopping trace sources, closing proxy and service processes and exiting...");

				Source.StopAll();
				Source.WaitAll();
//				Thread.Sleep(2000);

				TestProxy.Close();
//				TraceServiceProcess.Close();
//				TraceServiceProcess.WaitForExit();
			}
		}
	}
}

