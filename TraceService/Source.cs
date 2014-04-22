using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Runtime.Serialization;
using System.Reflection;
using Mono.Security.X509;
using System.Diagnostics;
using System.Text;
using Mono.CodeGeneration;

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
		private static readonly ConcurrentDictionary<string, Source> _sources = new ConcurrentDictionary<string, Source>();

		/// <summary>
		/// The exit all source threads.
		/// </summary>
		private static bool _exitAllThreads;

		public static readonly ConcurrentBag<Listener> GlobalListeners = new ConcurrentBag<Listener>();

		/// <summary>
		/// Gets the or create.
		/// </summary>
		/// <returns>The or create.</returns>
		/// <param name="listeners">Listeners.</param>
		public static Source GetOrCreate(params Listener[] listeners)
		{
			return Source.GetOrCreate(Assembly.GetExecutingAssembly().GetName().Name, listeners);
		}

		/// <summary>
		/// Gets the or create.
		/// </summary>
		/// <returns>The or create.</returns>
		/// <param name="name">Name.</param>
		public static Source GetOrCreate(string name, params Listener[] listeners)
		{
			return _sources.ContainsKey(name) ? _sources[name] : new Source(name, true, listeners);
		}

		/// <summary>
		/// Closes all.
		/// </summary>
		public static void StopAll()
		{
			_exitAllThreads = true;
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
		private Thread _thread;

		/// <summary>
		/// The exit source thread.
		/// </summary>
		private bool _exitThread;

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
			if (_sources.ContainsKey(name))
				throw new ArgumentException("Source name already in use", "name");
			Name = name;
			_messageQueue = new ConcurrentQueue<Message>();
			Listeners = new ConcurrentBag<Listener>(listeners);
			if (start)
				Start();
			_sources.TryAdd(name, this);
		}

		/// <summary>
		/// Start this instance.
		/// </summary>
		public void Start()
		{
			_thread = new Thread((thisService) => ((Source)thisService).Run())
			{
				Name = "TraceService.Source.Run",
				Priority = ThreadPriority.BelowNormal
			 };
			_thread.Start(this);
		}

		/// <summary>
		/// Close this instance.
		/// </summary>
		public void Stop()
		{
			_exitThread = true;
		}

		/// <summary>
		/// Wait for the source's run thread to finish
		/// </summary>
		public void Wait()
		{
			while (_thread != null)
				Thread.Sleep(LoopDelay);
		}

		/// <summary>
		/// Runs the service.
		/// </summary>
		internal virtual void Run()
		{
			Message message;
			while ((!_exitThread && !_exitAllThreads) || !_messageQueue.IsEmpty)
			{
				if (!_messageQueue.IsEmpty)
				{
					while (_messageQueue.TryDequeue(out message))
					{
						foreach (Listener listener in Listeners)
							lock (listener.SyncRoot)
							{
								listener.Trace(message);
							}
						foreach (Listener listener in GlobalListeners)
							lock (listener.SyncRoot)
							{
								listener.Trace(message);
							}
					}
				}
				else
					Thread.Sleep(LoopDelay);
			}
			_thread = null;
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
		/// Log the specified level, category, description and data.
		/// </summary>
		/// <param name="level">Level.</param>
		/// <param name="description">Description.</param>
		/// <param name="data">Data.</param>
		public void Log(MessageLevel level, string description, params object[] data)
		{
			Message msg = new Message(this, data)
			{
				Level = level,
				Description = description
			};
			Log(msg);
		}

		/// <summary>
		/// Log the specified level, category, description and data.
		/// </summary>
		/// <param name="level">Level.</param>
		/// <param name="data">Data.</param>
		public void Log(MessageLevel level, params object[] data)
		{
			Message msg = new Message(this, data)
			{
				Level = level
			};
			Log(msg);
		}

		/// <summary>
		/// Logs the method call.
		/// </summary>
		/// <param name="level">Level.</param>
		/// <param name="data">Data.</param>
		public void LogMethodCall(MessageLevel level, params object[] data)
		{
			StackFrame sf = new StackFrame(1, true);
			MethodBase method = sf.GetMethod();
			StringBuilder sb = new StringBuilder(256);
			sb.AppendFormat("{0}.{1}", method.DeclaringType.FullName, method.Name);
			if (method.ContainsGenericParameters)
			{
				sb.Append("<");
				foreach (Type TParam in method.GetGenericArguments())
					sb.AppendFormat("{0}, ", TParam.FullName);
				sb.Remove(sb.Length - 2, 2);
				sb.Append(">");
			}
			sb.Append("(");
			foreach (ParameterInfo pi in sf.GetMethod().GetParameters())
				sb.AppendFormat("{0} {1}, ", pi.ParameterType.FullName, pi.Name);
			sb.Remove(sb.Length - 2, 2);
			sb.Append(")");
			Message msg = new Message(this, data)
			{
				Level = level,
				Category = "Execution",
				Description = "Method Call"
			};
			Log(msg);
		}

		/// <summary>
		/// Logs the exception.
		/// </summary>
		/// <param name="ex">Ex.</param>
		/// <param name="data">Data.</param>
		public void LogException(Exception ex, params object[] data)
		{
			Message msg = new Message(this, data) {
				Level = MessageLevel.Error,
				Category = "Execution",
				Description = ex.ToString()		//.GetType().FullName
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

