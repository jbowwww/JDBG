using System;
using System.ServiceModel;
using System.IO;
using System.Xml;
using System.Text;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Serialization.Formatters.Binary;

namespace TraceService
{
	/// <summary>
	/// Trace service.
	/// </summary>
	[ServiceBehavior(IncludeExceptionDetailInFaults = true)]
	public class TraceService : Service, ITraceService, IDisposable
	{
		private FileStream _file;

		/// <summary>
		/// Initializes a new instance of the <see cref="TraceService.TraceService"/> class.
		/// </summary>
		public TraceService(string uri = "net.tcp://localhost:7777/Trace", IRemotingFormatter formatter = null)
			: base(new Uri(uri), formatter == null ? new BinaryFormatter() : formatter)
		{
			_file = File.Open("Trace.txt", FileMode.Create, FileAccess.ReadWrite, FileShare.Read);
		}

		/// <summary>
		/// Releases all resource used by the <see cref="TraceService.TraceService"/> object.
		/// </summary>
		/// <remarks>
		/// IDisposable implementation
		/// Call <see cref="Dispose"/> when you are finished using the <see cref="TraceService.TraceService"/>. The
		/// <see cref="Dispose"/> method leaves the <see cref="TraceService.TraceService"/> in an unusable state. After
		/// calling <see cref="Dispose"/>, you must release all references to the <see cref="TraceService.TraceService"/> so
		/// the garbage collector can reclaim the memory that the <see cref="TraceService.TraceService"/> was occupying.
		/// </remarks>
		public override void Dispose()
		{
			if (_file != null)
			{
				_file.Close();
				_file = null;
			}
		}

		/// <summary>
		/// Trace the specified message.
		/// </summary>
		/// <param name="message">Message.</param>
		/// <remarks>ITraceService implementation</remarks>
		public void Trace(TraceMessage message)
		{
			byte[] buf = Encoding.ASCII.GetBytes(message.ToString());
			_file.Write(buf, 0, buf.Length);
			_file.Flush();
		}

		/// <summary>
		/// Exits the trace service.
		/// </summary>
		public void ExitTraceService()
		{
			Stop();
		}
	}
}

