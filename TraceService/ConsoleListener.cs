using System;

namespace TraceService
{
	/// <summary>
	/// Console listener.
	/// </summary>
	public class ConsoleListener : Listener
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="TraceService.ConsoleListener"/> class.
		/// </summary>
		public ConsoleListener()
		{
		}

		/// <summary>
		/// Trace the specified message.
		/// </summary>
		/// <param name="message">Message.</param>
		/// <remarks>Listener implementation</remarks>
		public override void Trace(Message message)
		{
			Console.WriteLine(message.ToString());
		}
	}
}

