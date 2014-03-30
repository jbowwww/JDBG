using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Reflection;

namespace TraceService
{
	[Serializable]
	public class TraceMessage : ISerializable
	{
		public DateTime Time { get; protected set; }

		public string Message { get; protected set; }

		public IDictionary<string, object> Data { get; protected set; }

		public StackTrace Stack { get; protected set; }

//		[NonSerialized]
		public Process SourceProcess { get; protected set; }

//		[NonSerialized]
		public ProcessThread SourceThread { get; protected set; }

		public TraceMessage(string message)
		{
			Time = DateTime.Now;
			Message = message;
			Data = new Dictionary<string, object>();
			Stack = new StackTrace(1, true);

			SourceProcess = Process.GetCurrentProcess();
			SourceProcess.Refresh();
//			foreach (ProcessThread pt in SourceProcess.Threads)
//			{
//				if (pt.Id == Thread.CurrentThread.ManagedThreadId)
//				{
//					SourceThread = pt;
//					break;
//				}
//			}
		}

		protected TraceMessage(SerializationInfo info, StreamingContext context)
		{
			Time = info.GetDateTime("Time");
			Message = info.GetString("Message");
			Data = (IDictionary<string, object>)info.GetValue("Data", typeof(IDictionary<string, object>));
			Stack = (StackTrace)info.GetValue("Stack", typeof(StackTrace));
			SourceProcess = Process.GetProcessById(info.GetInt32("ProcessId"));
//			int threadId = info.GetInt32("ThreadId");
//			foreach (ProcessThread pt in SourceProcess.Threads)
//			{
//				if (pt.Id == threadId)
//				{
//					SourceThread = pt;
//					break;
//				}
//			}
		}

		/// <Docs>To be added: an object of type 'SerializationInfo'</Docs>
		/// <summary>
		/// To be added
		/// </summary>
		/// <param name="info">Info.</param>
		/// <param name="context">Context.</param>
		/// <remarks>ISerializable implementation</remarks>
		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("Time", Time);
			info.AddValue("Message", Message);
			info.AddValue("Data", Data);
			info.AddValue("Stack", Stack);
			info.AddValue("ProcessId", SourceProcess.Id);
//			info.AddValue("ThreadId", SourceThread.Id);
		}
//			MemberInfo[] members = FormatterServices.GetSerializableMembers(typeof(TraceMessage), context);
//			object[] memberValues = FormatterServices.GetObjectData(this, members);
//			for (int i = 0; i < members.Length; i++)
//			{
//				info.AddValue(members[i].Name, memberValues[i],
//					members[i].MemberType == MemberTypes.Field ?
//					(members[i] as FieldInfo).FieldType : (members[i] as PropertyInfo).PropertyType);
//			}
//			info.AddValue("ProcessId", SourceProcess.Id);
//			info.AddValue("ThreadId", SourceThread.ManagedThreadId);
//			info.SetType(this.GetType());
//		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder(string.Format("{0}: {1}:{2}/{3}: {4}\n", Time.ToString(),
				SourceProcess == null ? "Process=null" : SourceProcess.Site == null ? "Process.Site=null" : SourceProcess.Site.Name, 
				//SourceThread.Id, SourceThread.Site.Name,
				Thread.CurrentThread.ManagedThreadId, Thread.CurrentThread.Name, Message));
			if (Data != null && Data.Count > 0)
			{
				sb.AppendLine(string.Format("Data ({0} values):", Data.Count.ToString()));
				foreach (KeyValuePair<string, object> data in Data)
					sb.AppendLine(string.Format("\t{0}={1}", data.Key, data.Value.ToString()));
			}
			if (Stack != null)
			{
				sb.AppendLine("Stack:");
				sb.AppendLine(Stack.ToString());
			}
			return sb.ToString();
		}
	}
}

