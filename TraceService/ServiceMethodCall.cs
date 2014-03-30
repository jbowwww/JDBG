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
	public class ServiceMethodCall
	{
		public readonly Type ServiceType;

		public readonly string MethodName;

		public readonly object[] Arguments;

		public object Return { get; protected set; }

		public readonly MethodInfo Method;

		public bool HasReturn { get { return Method.ReturnParameter != null; } }

		public ServiceMethodCall(Type serviceType, string methodName, params object[] arguments)
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
			formatter.Serialize(stream, this);

			if (HasReturn)
			{
				BufferedStream bs = new BufferedStream(stream);

				Return = formatter.Deserialize(stream);
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
			ServiceMethodCall methodCall = (ServiceMethodCall)formatter.Deserialize(stream);
			methodCall.Return = methodCall.Method.Invoke(obj, methodCall.Arguments);
			if (methodCall.HasReturn)
			{
				formatter.Serialize(stream, methodCall.Return != null ?
					(methodCall.Return is string && (methodCall.Return as string).EndsWith("{null}")) ?
					string.Concat("/$/", methodCall.Return as string) : methodCall.Return : "{null}"); 
			}
		}
	}
}

