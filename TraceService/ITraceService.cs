using System;

namespace TraceService
{
	public interface ITraceService
	{
		void Trace(TraceMessage message);
		void ExitTraceService();
	}
}

