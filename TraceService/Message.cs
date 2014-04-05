using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;

namespace TraceService
{
	[DataContract]
	[KnownType(typeof(Dictionary<string, object>))]
	public class Message
	{
		[DataMember]
		public DateTime Time { get; set; }

		public TraceSource Source { get; set; }

		[DataMember]
		public MessageLevel Level { get; set; } 

		[DataMember]
		public int Id { get; set; }

		[DataMember]
		public string Category { get; set; }

		[DataMember]
		public string Description { get; set; }

//		[DataMember]
		public IDictionary<string, object> Data { get; set; }		//

//		[DataMember]
		public StackTrace Stack { get; set; }

//		[DataMember]
		public AppDomain SourceDomain { get; set; }

//		[DataMember]
		public Process SourceProcess { get; set; }

		[DataMember]
		public ProcessStartInfo ProcessInfo { get; set; }

		[DataMember]
		public string MachineName { get; set; }

		[DataMember]
		public int ProcessId { get; set; }

		[DataMember]
		public string ProcessName { get; set; }

//		[DataMember]
		public ProcessThread SourceThread { get; set; }

//		[DataMember]
		public Thread TraceThread { get; set; }

		[DataMember]
		public int ThreadId { get; set; }

		[DataMember]
		public string ThreadName { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="TraceService.Message"/> class.
		/// </summary>
		/// <param name="data">Data.</param>
		public Message(params object[] data)
		{
			Time = DateTime.Now;
			Stack = new StackTrace(1, true);

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

			SourceDomain = AppDomain.CurrentDomain;
			SourceProcess = Process.GetCurrentProcess();

			MachineName = SourceProcess.MachineName;

			ProcessId = SourceProcess.Id;
			ProcessName = SourceProcess.ProcessName;
			ProcessInfo = SourceProcess.StartInfo;
			
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