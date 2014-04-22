using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Runtime.Remoting.Messaging;
using System.Reflection;

namespace TraceService
{
	[MessageContract]
	[DataContract]
	[ServiceKnownType(typeof(System.Guid))]
	[KnownType(typeof(System.Guid))]
	public class Message
	{
		[MessageHeader]
		[DataMember]
		public DateTime Time { get; internal set; }

//		private AppDomain _domain;

//		[MessageHeader]
		public AppDomain Domain { get; set; }

		[MessageHeader]
		[DataMember]
		public string DomainName { get; set; }

		[MessageHeader]
		[DataMember]
		public int DomainId { get; set; }

//		[MessageHeader]
//		public long DomainSurvivedMemorySize { get; set; }
//
//		[MessageHeader]
//		public long DomainTotalMemorySize { get; set; }
//
//		[MessageHeader]
//		public TimeSpan DomainTotalProcessorTime { get; set; }

		[MessageHeader]
		[DataMember]
		public string DomainBaseDirectory { get; set; }

		[MessageHeader]
		[DataMember]
		public string DomainDynamicDirectory { get; set; }

			private Process _process;
//		[MessageHeader]
		public Process Process {
			get { return _process; }
			set
			{
				if ((_process = value) != null)
				{
					MachineName = _process.MachineName;
					ProcessId = _process.Id;
					ProcessName = _process.ProcessName;
				}
			}
		}

		[DataMember]
				[MessageHeader]
				public string MachineName { get; set; }

		[MessageHeader]
		[DataMember]
				public int ProcessId { get; set; }

		[MessageHeader]
		[DataMember]
		public string ProcessName { get; set; }

		public Thread Thread { get; set; }

		[MessageHeader]
		[DataMember]
		public int ThreadId { get; set; }

		[MessageHeader]
		[DataMember]
		public string ThreadName { get; set; }

		[MessageHeader]
		[DataMember]
		public ThreadPriority ThreadPriority { get; set; }

		[MessageHeader]
		[DataMember]
		public System.Threading.ThreadState ThreadState { get; set; }

		[MessageHeader]
		[DataMember]
		public Source Source { get; internal set; }

		[MessageHeader]
		[DataMember]
		public int Id { get; internal set; }

		[MessageHeader]
		[DataMember]
		public MessageLevel Level { get; set; } 

		[MessageHeader]
		[DataMember]
		public string Category { get; set; }

		[MessageHeader]
		[DataMember]
		public string Description { get; set; }

//		[MessageBodyMember]
//		[DataMember]
//		[KnownType(typeof(System.Guid))]
		public Dictionary<string, object> Data { get; set; }

//		[MessageBodyMember]
//		[DataMember]
		public StackTrace Stack { get; set; }

		[MessageBodyMember]
		[DataMember]
		public byte[] BinaryData {
			get
			{
				BinaryFormatter bf = new BinaryFormatter(null, new StreamingContext(StreamingContextStates.Remoting));
				using (MemoryStream ms = new MemoryStream())
				{
					bf.Serialize(ms, Data);
					bf.Serialize(ms, Stack);
					return ms.ToArray();
				}
			}
			set
			{
				BinaryFormatter bf = new BinaryFormatter(null, new StreamingContext(StreamingContextStates.Remoting));
				using (MemoryStream ms = new MemoryStream(value))
				{
					Data = (Dictionary<string, object>)bf.Deserialize(ms);
					Stack = (StackTrace)bf.Deserialize(ms);
				}
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="TraceService.Message"/> class.
		/// </summary>
		internal Message()
		{
			;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="TraceService.Message"/> class.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="data">Data.</param>
		public Message(Source source, params object[] data)
		{
			Time = DateTime.Now;
			Domain = AppDomain.CurrentDomain;
			DomainId = Domain.Id;
			DomainName = Domain.FriendlyName;
//			DomainSurvivedMemorySize = Domain.MonitoringSurvivedMemorySize;
//			DomainTotalMemorySize = Domain.MonitoringTotalAllocatedMemorySize;
//			DomainTotalProcessorTime = Domain.MonitoringTotalProcessorTime;
			DomainBaseDirectory = Domain.BaseDirectory;
			DomainDynamicDirectory = Domain.DynamicDirectory;

			Process = Process.GetCurrentProcess();
			Thread = Thread.CurrentThread;
			ThreadId = Thread.ManagedThreadId;
			ThreadName = Thread.Name;
			ThreadPriority = Thread.Priority;
			ThreadState = Thread.ThreadState;
			Source = source;
			Id = source.NextMessageId;
			Level = MessageLevel.Information;
			Category = Description = string.Empty;			// Category and Description can be set if desired using property assignment (ie c'tor(...) { Category = ... , Description = ... })
			Stack = new StackTrace(2, true);
			Data = new Dictionary<string, object>();
			foreach (object d in data)
			{
				Type dType = d.GetType();
				if (dType.IsGenericType && dType.GetGenericTypeDefinition().IsEquivalentTo(
					typeof(KeyValuePair<object,object>).GetGenericTypeDefinition()))
				{
					KeyValuePair<object, object> dPair = (KeyValuePair<object, object>)d;
					Data.Add(dPair.Key.ToString(), dPair.Value);
				}
				else
					Data.Add(string.Format("Data{0:d2}", Data.Count), d);
			}
		}

//		#region Serialization callbacks
//		[OnSerializing]
//		public void OnSerialize(StreamingContext context)
//		{
//			BinaryFormatter bf = new BinaryFormatter(null, context);
//			using (MemoryStream ms = new MemoryStream())
//			{
//				bf.Serialize(ms, Data);
//				bf.Serialize(ms, Stack);
//				_binaryData = ms.ToArray();
//			}
//		}
//
//		[OnDeserializing]
//		public void OnDeserialize(StreamingContext context)
//		{
//			BinaryFormatter bf = new BinaryFormatter(null, context);
//			using (MemoryStream ms = new MemoryStream(_binaryData))
//			{
//				Data = (Dictionary<string, object>)bf.Deserialize(ms);
//				Stack = (StackTrace)bf.Deserialize(ms);
//			}
//		}
//		#endregion

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
			return ToString(false);
		}

		/// <summary>
		/// Returns a <see cref="System.String"/> that represents the current <see cref="TraceService.Message"/>.
		/// </summary>
		/// <returns>A <see cref="System.String"/> that represents the current <see cref="TraceService.Message"/>.</returns>
		public string ToString(bool verbose = false)
		{
			StringBuilder sb;
			if (_toString != null)
				sb = new StringBuilder(_toString, 1024);
			else
			{
				StackFrame frame = Stack.GetFrame(0);
				MethodBase method = frame.GetMethod();
				string source = string.Format("{0}:{1},{2} (+0x{3:x})", frame.GetFileName(), frame.GetFileLineNumber(), frame.GetFileColumnNumber(), frame.GetILOffset());
				sb = new StringBuilder(1024);
				sb.AppendFormat("{0}: {1}/{2}: {3}: {4}/{5}: {6}/{7}: {8}: {9}: {10} {11} ({12}) {13}{14}", Time, DomainId, DomainName,			// TODO: Null reference exceptions happening here
					MachineName, ProcessId, ProcessName, ThreadId, ThreadName, Source.Name,
					string.Concat(method.DeclaringType, ".", method.Name), Id, Level, source,
					string.IsNullOrWhiteSpace(Category) ? string.Empty : string.Concat(Category, " "), Description);
				_toString = sb.ToString();
			}
			if (verbose)
			{
				if (Data != null && Data.Count > 0)
				{
					sb.AppendFormat("\nData ({0} values):", Data.Count);
					foreach (KeyValuePair<string, object> data in Data)
						sb.AppendFormat("\n\t{0}={1}", data.Key, data.Value == null ? "(null)" : data.Value.ToString());
				}
				if (Stack != null)
					sb.AppendFormat("\nStack ({0} frames):\n{1}", Stack.FrameCount, Stack.ToString());
			}
			return sb.ToString();
		}
	}
}