using System;
using System.Linq;
using System.Reflection;
using System.ServiceModel.Channels;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Remoting.Messaging;
using System.Net.Sockets;
using System.IO;

namespace TraceService
{
	[Serializable]
	public class MethodCall
	{
		public readonly Type ServiceType;

		public readonly string MethodName;

		public readonly object[] Arguments;

		public object Return { get; protected set; }

		public readonly MethodInfo Method;

		public bool HasReturn {
			get { return Method.ReturnParameter != null && !Method.ReturnParameter.ParameterType.Equals(typeof(void)); }
		}

		public MethodCall(Type serviceType, string methodName, params object[] arguments)
		{
			ServiceType = serviceType;
			MethodName = methodName;
			Arguments = arguments;
			Method = ServiceType.GetMethods().FirstOrDefault(
				(mi) => mi.Name == MethodName && mi.GetParameters().Count() == Arguments.Length);
			if (Method == null)
				throw new MemberAccessException(string.Format("Could not get method {0}.{1}", ServiceType.FullName, MethodName));
		}

		internal void InvokeFromProxy(IRemotingFormatter formatter, NetworkStream stream)
		{
			MemoryStream ms = new MemoryStream();
			formatter.Serialize(ms, this);
			IAsyncResult async = stream.BeginWrite(ms.GetBuffer(), 0, (int)ms.Length, null, null);
			if (HasReturn)
			{
				stream.EndWrite(async);
//				vc		ms = new MemoryStream(stream.
//						IAsyncResult asyncR = stream.c
				Return = formatter.Deserialize(stream);		// Determine if this deseerializstaion need t be async too - see if this syn read bottlenecks
				if (Return is string)
				{
					if ((Return as string).Equals("{null}"))
						Return = null;
					else if ((Return as string).EndsWith("{null}"))
						Return = (Return as string).Substring(3);			// remove leading "/$/"
				}
			}

		}

		internal static void InvokeMethod(IRemotingFormatter formatter, NetworkStream stream, object obj)
		{
			MethodCall methodCall = (MethodCall)formatter.Deserialize(stream);
			methodCall.Return = methodCall.Method.Invoke(obj, methodCall.Arguments);
			if (methodCall.HasReturn)
			{
				object r = methodCall.Return != null ?
				           (methodCall.Return is string && (methodCall.Return as string).EndsWith("{null}")) ?
				           string.Concat("/$/", methodCall.Return as string) : methodCall.Return : "{null}";
				MemoryStream ms = new MemoryStream();
				formatter.Serialize(ms, r); 
				stream.BeginWrite(ms.GetBuffer(), 0, (int)ms.Length, null, null);
			}
		}
	}
}

