using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Runtime.Serialization;

namespace TraceService
{
	/// <summary>
	/// A source for use with <see cref="TraceService.TraceService"/>, similar to <see cref="System.Diagnostics.TraceSource"/>
	/// </summary>
	/// <remarks> 
	/// When events are logged, a <see cref="TraceService.TraceMessage"/> is created and placed in a queue.
	/// Each <see cref="TraceService.TraceService"/> instance runs a dedicated low priority thread that monitors this queue.
	/// This design aims to minimise CPU time spent on tracing by the main thread(s).
	/// For each <see cref="TraceService.TraceMessage"/> it takes from the queue, the message is passed to a collection of
	/// <see cref="TraceService.TraceServiceListener"/>s. One of the available types derived from this listener class is
	/// <see cref="TraceServiceProxyListener"/>, which serializes the message to a <see cref="TraceService.TraceService"/> .
	/// <see cref="TraceService.TraceService"/> can also contain multiple <see cref="TraceService.TraceServiceListener"/>s,
	/// allowing multiple listeners to be used in combination, on both the client and server side, efficiently.
	/// !! TODO
	/// </remarks>
	[DataContract]
	public class Source
	{
		#region Static members
		/// <summary>
		/// The _sources.
		/// </summary>
		private static ConcurrentDictionary<string, Source> _sources;

		/// <summary>
		/// The exit all source threads.
		/// </summary>
		private static bool ExitAllSourceThreads;

		/// <summary>
		/// Gets the or create.
		/// </summary>
		/// <returns>The or create.</returns>
		/// <param name="name">Name.</param>
		public static Source GetOrCreate(string name, bool start = true, params Listener[] listeners)
		{
			if (_sources == null)
				_sources = new ConcurrentDictionary<string, Source>();
			if (!_sources.ContainsKey(name))
				_sources.TryAdd(name, new Source(name, start, listeners));
			return _sources[name];
		}

		/// <summary>
		/// Closes all.
		/// </summary>
		public static void StopAll()
		{
			ExitAllSourceThreads = true;
		}

		/// <summary>
		/// Waits for all Sources' run threads to finish
		/// </summary>
		public static void WaitAll()
		{
			foreach (Source source in _sources.Values)
				source.Wait();
		}
		#endregion

		#region Private fields
		/// <summary>
		/// The _message queue.
		/// </summary>
		private ConcurrentQueue<Message> _messageQueue;

		/// <summary>
		/// The source thread.
		/// </summary>
		private Thread SourceThread;

		/// <summary>
		/// The exit source thread.
		/// </summary>
		private bool ExitSourceThread;

		/// <summary>
		/// The _next message identifier.
		/// </summary>
		private int _nextMessageId = 0;
		#endregion

		/// <summary>
		/// The loop delay.
		/// </summary>
		public const int LoopDelay = 50;

		/// <summary>
		/// The name.
		/// </summary>
		[DataMember]
		public readonly string Name;

		/// <summary>
		/// Gets the next message identifier.
		/// </summary>
		/// <value>The next message identifier.</value>
		internal int NextMessageId {
			get { return _nextMessageId++; }
		}

		/// <summary>
		/// The _sources.
		/// </summary>
		public readonly ConcurrentBag<Listener> Listeners;

		/// <summary>
		/// Initializes a new instance of the <see cref="TraceService.Source"/> class.
		/// </summary>
		/// <param name="name">Name.</param>
		public Source(string name, bool start = true, params Listener[] listeners)
		{
			Name = name;
			_messageQueue = new ConcurrentQueue<Message>();
			Listeners = new ConcurrentBag<Listener>(listeners);
			if (start)
				Start();
		}

		/// <summary>
		/// Start this instance.
		/// </summary>
		public void Start()
		{
			SourceThread = new Thread((thisService) => ((Source)thisService).Run())
			{
				Name = "TraceService.Source.Run",
				Priority = ThreadPriority.BelowNormal
			 };
			SourceThread.Start(this);
		}

		/// <summary>
		/// Close this instance.
		/// </summary>
		public void Stop()
		{
			ExitSourceThread = true;
		}

		/// <summary>
		/// Wait for the source's run thread to finish
		/// </summary>
		public void Wait()
		{
			while (SourceThread != null)
				Thread.Sleep(LoopDelay);
		}

		/// <summary>
		/// Runs the service.
		/// </summary>
		public void Run()
		{
			Message message;
			while ((!ExitSourceThread && !ExitAllSourceThreads) || !_messageQueue.IsEmpty)
			{
				if (!_messageQueue.IsEmpty)
				{
					while (_messageQueue.TryDequeue(out message))
						foreach (Listener listener in Listeners)
							listener.Trace(message);
				}
				else
					Thread.Sleep(LoopDelay);
			}
			SourceThread = null;
		}

		/// <summary>
		/// Log the specified level, category, description and data.
		/// </summary>
		/// <param name="level">Level.</param>
		/// <param name="category">Category.</param>
		/// <param name="description">Description.</param>
		/// <param name="data">Data.</param>
		public void Log(MessageLevel level, string category, string description, params object[] data)
		{
			Message msg = new Message(this, data)
			{
				Level = level,
				Category = category,
				Description = description
			};
			Log(msg);
		}

		/// <summary>
		/// Log the specified message.
		/// </summary>
		/// <param name="message">Message.</param>
		private void Log(Message message)
		{
			_messageQueue.Enqueue(message);
		}
	}
}

