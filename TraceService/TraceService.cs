using System;
using System.ServiceModel;
using System.IO;
using System.Text;
using System.Runtime.Serialization;
using System.Reflection;

namespace TraceService
{
	/// <summary>
	/// Trace service.
	/// </summary>
	[ServiceBehavior(IncludeExceptionDetailInFaults = true)]
	public class TraceService : Disposable, ITraceService
	{
		private FileStream _file;

		/// <summary>
		/// Initializes a new instance of the <see cref="TraceService.TraceService"/> class.
		/// </summary>
		public TraceService()
		{
			_file = File.Open("Trace.txt", FileMode.Create, FileAccess.ReadWrite, FileShare.Read);
			Console.WriteLine("Service: Opened file \"{0}\"", _file.Name);
		}

		/// <summary>
		/// Releases unmanaged resources and performs other cleanup operations before the
		/// <see cref="TraceService.TraceService"/> is reclaimed by garbage collection.
		/// </summary>
		~TraceService()
		{
			Dispose(false);
		}

		/// <summary>
		/// Close this instance.
		/// </summary>
		public void Close()
		{
			Dispose(true);
		}

		/// <summary>
		/// Disposes the unmanaged resources
		/// </summary>
		protected override void DisposeManaged()
		{
			if (_file != null)
			{
				Console.WriteLine("Service: {0}: Closing file \"{1}\"", DateTime.Now, _file.Name);
				_file.Close();
				_file = null;
			}
		}

		/// <summary>
		/// Loads the assembly.
		/// </summary>
		/// <param name="path">Path.</param>
		public void LoadAssembly(string path)
		{
			Assembly.LoadFile(path);
		}

		/// <summary>
		/// Trace the specified message.
		/// </summary>
		/// <param name="message">Message.</param>
		/// <remarks>ITraceService implementation</remarks>
		public void Trace(Message message)
		{
			Console.WriteLine("Service: {0}: Writing \"{1}\"", DateTime.Now, message.ToString());
			byte[] buf = Encoding.ASCII.GetBytes(string.Concat(message.ToString(), "\n"));
			_file.Write(buf, 0, buf.Length);
			_file.Flush();
		}
	}
}

