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
//		public static ChannelFactory<ITraceService> Factory;

		public static TraceServiceProxy Trace;

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
			Trace = new TraceServiceProxy(new BinaryFormatter());
				TraceMessage tm = new TraceMessage("Test Trace");
				tm.Data.Add("value1", new object());
			Trace.Trace(tm);
				Trace.ExitTraceService();
				Thread.Sleep(3000);
//			}
//			}

			Console.WriteLine("Exiting client...");
		}
	}
}

