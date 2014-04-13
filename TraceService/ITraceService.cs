using System;
using System.ServiceModel;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace TraceService
{
	[ServiceContract]
	[ServiceKnownType(typeof(System.Guid))]
	public interface ITraceService
	{
		[OperationContract]
		void LoadAssembly(string path);

		[OperationContract]
		[ServiceKnownType(typeof(System.Guid))]
		void Trace(Message message);
	}
}

