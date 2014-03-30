using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Text;

namespace TraceService
{
	[Serializable]
	public class TraceMessage// : ISerializable
	{
		public DateTime Time { get; protected set; }

		public string Message { get; protected set; }

		public IDictionary<string, object> Data { get; protected set; }

		public StackTrace Stack { get; protected set; }

		public TraceMessage(string message)
		{
			Time = DateTime.Now;
			Message = message;
			Stack = new StackTrace(1, true);
			Data = new Dictionary<string, object>();
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder(string.Format("{0} {1}\n", Time.ToString(), Message));
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

