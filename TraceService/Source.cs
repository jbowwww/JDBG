using System;
using System.Collections.Concurrent;
using System.Threading;

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
	public class Source
	{
		/// <summary>
		/// The loop delay.
		/// </summary>
		public const int LoopDelay = 100;

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
		public static Source GetOrCreate(string name, bool start = false, params Listener[] listeners)
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
		public static void CloseAll()
		{
			ExitAllSourceThreads = true;
		}

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
		/// The _sources.
		/// </summary>
		public readonly ConcurrentBag<Listener> Listeners;

		/// <summary>
		/// Initializes a new instance of the <see cref="TraceService.Source"/> class.
		/// </summary>
		/// <param name="name">Name.</param>
		public Source(string name, bool start = true, params Listener[] listeners)
		{
			_messageQueue = new ConcurrentQueue<Message>();
			Listeners = new ConcurrentBag<Listener>(listeners);
			//, SourceLevels levels = SourceLevels.All)
			if (start)
				Start();
		}

		/// <summary>
		/// Releases all resource used by the <see cref="TraceService.Source"/> object.
		/// </summary>
		/// <remarks>
		/// IDisposable implementation
		/// Call <see cref="Dispose"/> when you are finished using the <see cref="TraceService.Source"/>. The
		/// <see cref="Dispose"/> method leaves the <see cref="TraceService.Source"/> in an unusable state. After calling
		/// <see cref="Dispose"/>, you must release all references to the <see cref="TraceService.Source"/> so the garbage
		/// collector can reclaim the memory that the <see cref="TraceService.Source"/> was occupying.
		/// </remarks>
		public virtual void Dispose()
		{
			if (SourceThread != null)
			{
				if (SourceThread.IsAlive)
				{
					Stop();
					SourceThread.Join();
				}
				SourceThread = null;
			}
		}

		/// <summary>
		/// Close this instance.
		/// </summary>
		public void Stop()
		{
			ExitSourceThread = true;
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
		/// Runs the service.
		/// </summary>
		public void Run()
		{
			while (!ExitSourceThread && !ExitAllSourceThreads)
			{
				if (_messageQueue.IsEmpty)
					Thread.Sleep(LoopDelay);
				else
				{
					Message message;
					while (_messageQueue.TryDequeue(out message))
					{
//					Listener[] listeners = _listeners.ToArray();
						foreach (Listener listener in Listeners)
						{
							listener.Trace(message);
						}
					}
				}
			}
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
			Message msg = new Message(data) { Level = level, Category = category, Description = description };
			_messageQueue.Enqueue(msg);
		}
	}
}

