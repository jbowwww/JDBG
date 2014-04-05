using System;
using System.ServiceModel;
using System.Diagnostics;

namespace TraceService
{
	[ServiceContract]
	public interface ITraceService
	{
		[OperationContract]
		void Trace(Message message);
	}
}

