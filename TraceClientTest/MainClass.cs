using System;
using TraceService;
using System.Diagnostics;
using System.Threading;
using Mono.Posix;
using System.Collections.Generic;
using System.ServiceModel;
using System.Runtime.Serialization.Formatters.Binary;

namespace TraceClientTest
{
	public static class MainClass
	{
		public readonly static Source Trace = Source.GetOrCreate("TraceClientTest", true, new ConsoleListener());
			public static TraceProxy TestService;

		public static Process TraceServiceProcess;

		public static void Main(string[] argv)
		{
//			TraceServiceProcess = Process.Start("../../../TraceService/bin/Debug/TraceService.exe");

			new Thread(() => { TraceService.MainClass.Main(new string[] { }); }).Start();

			Thread.Sleep(3000);

//			if (TraceServiceProcess == null)
//				Console.WriteLine("Could not start service");
//			else
//			{
				Console.WriteLine("Service started, opening client proxy...");
			TestService = new TraceProxy(new BinaryFormatter());
//				Message tm = new Message("Test Trace");
//				tm.Data.Add("value1", new object());
			Trace.Log(MessageLevel.Information, "", "Testicles 1 2", new Guid());//(tm);
			TestService.ExitTraceService();
				Thread.Sleep(3000);
//			}
//			}

			Console.WriteLine("Exiting client & TraceService.Sourecs...");
			Source.CloseAll();
		}
	}
}

