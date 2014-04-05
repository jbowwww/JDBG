using System;
using System.Runtime.Serialization;
using System.Threading;
using System.Collections.Generic;
using System.Diagnostics;

namespace TraceService
{
	/// <summary>
	/// Surrogate for <see cref="TraceService.Message"/>
	/// </summary>
	public class MessageSurrogated
	{
		[DataMember]
		public DateTime Time { get; internal set; }

		public readonly Source Source { get; internal set; }

		[DataMember]
		public string SourceName { get; private set; }

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
		public object[] DataValues { get; set; }

		public StackTrace Stack { get; set; }

		public AppDomain Domain { get; set; }

		public Process Process { get; set; }

		[DataMember]
		public int ProcessId { get; set; }

		[DataMember]
		public string ProcessName { get; set; }

		public string MachineName { get; set; }

		public Thread TraceThread { get; set; }

		[DataMember]
		public int ThreadId { get; set; }

		[DataMember]
		public string ThreadName { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="TraceService.MessageSurrogated"/> class.
		/// </summary>
		/// <param name="message">Message.</param>
		public MessageSurrogated(Message message)
		{
			Time = message.Time;
			Source = message.Source;
			SourceName = message.Source.Name;
			Level = message.Level;
			Id = message.Id;
			Category = message.Category;
			Description = message.Description;
			Data = message.Data;
			DataValues = new object[Data.Count];
			message.Data.Values.CopyTo(DataValues, 0);
			Stack = message.Stack;
			Domain = message.Domain;
			Process = message.Process;
			ProcessId = message.Process.Id;

		}
	}
}

