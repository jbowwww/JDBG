using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;

namespace TraceService
{
	public class Message
	{
		public DateTime Time { get; internal set; }

		public readonly Source Source { get; internal set; }

		public MessageLevel Level { get; set; } 

		public int Id { get; internal set; }

		public string Category { get; set; }

		public string Description { get; set; }

		public IDictionary<string, object> Data { get; set; }

		public StackTrace Stack { get; set; }

		public AppDomain Domain { get; set; }

		public Process Process { get; set; }

		public string MachineName { get; set; }

//		public ProcessThread SourceThread { get; set; }

		public Thread TraceThread { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="TraceService.Message"/> class.
		/// </summary>
		/// <param name="data">Data.</param>
		public Message(params object[] data)
		{
			Time = DateTime.Now;
			Stack = new StackTrace(1, true);
			Level = MessageLevel.Information;

			int di = 0;
			Data = new Dictionary<string, object>();
			foreach (object d in data)
			{
				di++;
				Type dType = d.GetType();
				if (dType.IsGenericType && dType.GetGenericTypeDefinition().IsEquivalentTo(
					typeof(KeyValuePair<object,object>).GetGenericTypeDefinition()))
				{
					KeyValuePair<object, object> dPair = (KeyValuePair<object, object>)d;
					Data.Add(dPair.Key.ToString(), dPair.Value);
				}
				else
					Data.Add(string.Format("Data{0:d2}", di), d);
			}

			Domain = AppDomain.CurrentDomain;
			Process = Process.GetCurrentProcess();

			MachineName = Process.MachineName;

			ProcessId = Process.Id;
			ProcessName = Process.ProcessName;
			ProcessInfo = Process.StartInfo;
			
//			SourceProcess.Refresh();
//			foreach (ProcessThread pt in SourceProcess.Threads)
//			{
//				if (pt != null && pt.Id == Thread.CurrentThread.ManagedThreadId)
//				{
//					SourceThread = pt;
//					break;
//				}
//			}

			TraceThread = Thread.CurrentThread;
			ThreadId = TraceThread.ManagedThreadId;
			ThreadName = TraceThread.Name;
		}

		/// <summary>
		/// The _to string.
		/// </summary>
		private string _toString = null;

		/// <summary>
		/// Returns a <see cref="System.String"/> that represents the current <see cref="TraceService.Message"/>.
		/// </summary>
		/// <returns>A <see cref="System.String"/> that represents the current <see cref="TraceService.Message"/>.</returns>
		public override string ToString()
		{
			if (_toString != null)
				return _toString;

			StringBuilder sb = new StringBuilder(string.Format("{0}: {1}: {2}/{3}: {4}/{5}: {6}\n", Time.ToString(),
				MachineName, ProcessId.ToString(), ProcessName, ThreadId, ThreadName, Description ?? string.Empty));
			
			if (Data != null && Data.Count > 0)
			{
				sb.AppendLine(string.Format("Data ({0} values):", Data.Count.ToString()));
				foreach (KeyValuePair<string, object> data in Data)
					sb.AppendLine(string.Format("\t{0}={1}", data.Key, data.Value == null ? "(null)" : data.Value.ToString()));
			}
			if (Stack != null)
			{
				sb.AppendLine("Stack:");
				sb.Append(Stack.ToString());
			}
			return _toString = Stack == null ? sb.ToString(0, sb.Length - 1) : sb.ToString();
		}
	}
}