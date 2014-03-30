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
	public class Message : ISerializable
	{
		public uint Id { get; internal set; }

		public Source TraceSource { get; internal set; }

		public DateTime Time { get; protected set; }

		public MessageLevel Level { get; internal set; }													//

		public string Category { get; internal set; }															//

		public string Description { get; internal set; }														//

		public IDictionary<string, object> Data { get; protected set; }		//

		public StackTrace Stack { get; protected set; }

		public AppDomain SourceDomain { get; protected set; }

		public Process SourceProcess { get; protected set; }

		public ProcessThread SourceThread { get; protected set; }

		public Thread TraceThread { get; protected set; }

		public int ThreadId { get; protected set; }

		public string ThreadName { get; protected set; }

		public Message(params object[] data)		// string message)
		{
			Time = DateTime.Now;
//			Message = message;
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
			SourceProcess.Refresh();
			
//			foreach (ProcessThread pt in SourceProcess.Threads)
//			{
//				if (pt.Id == Thread.CurrentThread.ManagedThreadId)
//				{
//					SourceThread = pt;
//					break;
//				}
//			}
			TraceThread = Thread.CurrentThread;
			ThreadId = TraceThread.ManagedThreadId;
			ThreadName = TraceThread.Name;
		}

		protected Message(SerializationInfo info, StreamingContext context)
		{
			Time = info.GetDateTime("Time");
			Description = info.GetString("Message");
			Data = (IDictionary<string, object>)info.GetValue("Data", typeof(IDictionary<string, object>));
			Stack = (StackTrace)info.GetValue("Stack", typeof(StackTrace));

			SourceProcess = Process.GetProcessById(info.GetInt32("ProcessId"));
			ThreadId = info.GetInt32("ThreadId");
			ThreadName = info.GetString("ThreadName");

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
			info.AddValue("Message", Description);
			info.AddValue("Data", Data);
			info.AddValue("Stack", Stack);
			info.AddValue("ProcessId", SourceProcess.Id);
			info.AddValue("ThreadId", SourceThread != null ? SourceThread.Id : TraceThread != null ? TraceThread.ManagedThreadId : 0);
			info.AddValue("ThreadName", TraceThread.Name);
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

		private string _toString = null;
		public override string ToString()
		{
			if (_toString != null)
				return _toString;
			StringBuilder sb = new StringBuilder(string.Format("{0}: {1}: {2}/{3}: {4}/{5}: {6}\n", Time.ToString(),
				SourceProcess.MachineName, SourceProcess.Id, SourceProcess.ProcessName, ThreadId, ThreadName, Description));
//				SourceProcess == null ? "Process=null" : SourceProcess..Site == null ? "Process.Site=null" : SourceProcess.Site.Name, 
				//SourceThread.Id, SourceThread.Site.Name,
			
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
			return _toString = sb.ToString();
		}
	}
}

