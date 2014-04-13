using System;
using System.Threading;
using System.Runtime.Serialization;

namespace TraceService
{
	[DataContract]
	public class ThreadSurrogated
	{
		[DataMember]
		public int Id;

		[DataMember]
		public string Name;

		[DataMember]
		public ThreadPriority Priority;

		[DataMember]
		public ThreadState State;

		[DataMember]
		public bool IsBackground;

		[DataMember]
		public bool IsThreadPoolThread;

		public ThreadSurrogated(Thread thread)
		{
			Id = thread.ManagedThreadId;
			Name = thread.Name;
			Priority = thread.Priority;
			State = thread.ThreadState;
			IsBackground = thread.IsBackground;
			IsThreadPoolThread = thread.IsThreadPoolThread;
		}
	}
}

