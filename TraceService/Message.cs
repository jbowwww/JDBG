using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Reflection;

namespace TraceService
{
	[DataContract]
	public class Message
	{
		[DataMember]
		public DateTime Time { get; internal set; }

		public Source Source { get; internal set; }

		[DataMember]
		public MessageLevel Level { get; set; } 

		[DataMember]
		public int Id { get; internal set; }

		[DataMember]
		public string Category { get; set; }

		[DataMember]
		public string Description { get; set; }

		public IDictionary<string, object> Data { get; set; }

		[DataMember]
		public MemoryStream DataStream { get; internal set; }

		public byte[] DataBytes { get; set; }

		public StackTrace Stack { get; set; }

		public AppDomain Domain { get; set; }

		[DataMember]
		public string MachineName { get; set; }

		public Process Process { get; set; }

		[DataMember]
		public int ProcessId { get; set; }
//			get { return Process == null ? 0 : Process.Id; }
//			set { Process = value <= 0 ? null : Process.GetProcessById(value, MachineName); }
//		}x

//		[DataMember]
		public string ProcessName {
			get { return Process == null ? string.Empty : Process.ProcessName; }
		}

		public Thread TraceThread { get; set; }

		[DataMember]
		public string ThreadId {
			get { return _threadId ?? TraceThread.ManagedThreadId.ToString(); }
			set { _threadId = value; }
		}
		private string _threadId;
		
//		[DataMember]
		public string ThreadName {
			get { return TraceThread == null ? string.Empty : TraceThread.Name; }
		}

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
			ProcessId = Process.Id;

			MachineName = Process.MachineName;
			TraceThread = Thread.CurrentThread;
		}

		[OnSerializing]
		public void OnSerialize(StreamingContext context)
		{
//			using (DataStream = new MemoryStream(4096))
//			{
			DataStream = new MemoryStream(4096);
			BinaryFormatter bf = new BinaryFormatter(null, context);
				bf.Serialize(DataStream, Data);
//				bf.Serialize(DataStream, Stack);
//				DataBytes = DataStream.ToArray();
//			}
		}

		[OnDeserializing]
		public void OnDeserialize(StreamingContext context)
		{
			Console.WriteLine("DataStream is {0} bytes long", DataStream == null ? "(null)" : DataStream.Length.ToString());

//			using (DataStream = new MemoryStream(DataBytes))
//			{
			BinaryFormatter bf = new BinaryFormatter(null, context);
				Data = (IDictionary<string, object>)bf.Deserialize(DataStream);
//				Stack = (StackTrace)bf.Deserialize(DataStream);
//			}
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