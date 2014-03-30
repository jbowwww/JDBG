using System;

namespace TraceService
{
	public interface ITraceService
	{
		void Trace(Message message);
		void ExitTraceService();
	}
}

