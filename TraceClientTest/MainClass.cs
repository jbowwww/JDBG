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
			using (TraceServiceProcess = Process.Start("../../../TraceService/bin/Debug/TraceService.exe"))		//	new Thread(() => { TraceService.MainClass.Main(new string[] { }); }).Start(); 
			{
				Console.WriteLine("Service Process: Started: {0}", TraceServiceProcess.StartTime);
				TraceServiceProcess.OutputDataReceived += (sender, e) => Console.WriteLine("Service Process: Output Data Received: {0}: {1}", sender, e.Data);
				TraceServiceProcess.ErrorDataReceived += (sender, e) => Console.WriteLine("Service Process: Error Data Received: {0}: {1}", sender, e.Data);
        		TraceServiceProcess.Disposed += (sender, e) => Console.WriteLine("Service Process: Disposed");
				TraceServiceProcess.Exited += (sender, e) => Console.WriteLine("Service Process: Exited: {0}: {1}", TraceServiceProcess.ExitCode, TraceServiceProcess.ExitTime);

				Thread.Sleep(1100);
				Console.ReadLine();
				Console.WriteLine("Client: Creating proxy...");
				using (TestProxy = new TraceProxy(Binding, new EndpointAddress(BaseUri)))
				{
					Trace.Listeners.Add(new TraceProxyListener(TestProxy));

					Trace.Log(MessageLevel.Information, "Test", "Testicles 1 2", new Guid());
					Trace.Log(MessageLevel.Verbose, "Test", "Bitses", new BitArray(32));

					Source.StopAll();
					Source.WaitAll();

					Console.WriteLine("Client: Stopped trace sources, closing proxy");

					TestProxy.Close();
				}

				Console.WriteLine("Client: Stopping service processes and exiting...");

				TraceServiceProcess.Close();
				TraceServiceProcess.WaitForExit();

			}
		}
	}
}

