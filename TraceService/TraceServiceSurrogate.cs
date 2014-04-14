using System;
using System.Runtime.Serialization;
using System.Diagnostics;
using System.Threading;

namespace TraceService
{
	public class TraceServiceSurrogate : IDataContractSurrogate
	{
		public TraceServiceSurrogate()
		{
		}

		#region IDataContractSurrogate implementation

		public object GetCustomDataToExport(System.Reflection.MemberInfo memberInfo, Type dataContractType)
		{
			throw new NotImplementedException();
		}

		public object GetCustomDataToExport(Type clrType, Type dataContractType)
		{
			throw new NotImplementedException();
		}

		public Type GetDataContractType(Type type)
		{
			if (typeof(Thread).IsAssignableFrom(type))
				return typeof(ThreadSurrogated);
			return type;
		}

//			if (typeof(Message).IsAssignableFrom(type))
//				return typeof(MessageSurrogated);
//			if (typeof(Source).IsAssignableFrom(type))
//				return typeof(SourceSurrogated);
//			if (typeof(StackTrace).IsAssignableFrom(type))
//				return typeof(StackTraceSurrogated);
//			if (type.IsInterface && type.IsGenericType
//				&&  type.GetGenericArguments().Length == 2
//				&& type.Name.StartsWith("IDictionary"))
//			{
//				Type[] genericArgs = type.GetGenericArguments();
//				return typeof(DictionarySurrogated<object,object>).GetGenericTypeDefinition().MakeGenericType(genericArgs[0], genericArgs[1]);
//			}
//			if (typeof(AppDomain).IsAssignableFrom(type))
//				return typeof(AppDomainSurrogated);
//			if (typeof(Process).IsAssignableFrom(type))
//				return typeof(ProcessSurrogated);
//			if (typeof(Thread).IsAssignableFrom(type))
//				return typeof(ThreadSurrogated);
//			return type;
//		}

		public object GetDeserializedObject(object obj, Type targetType)
		{
			throw new NotImplementedException();
		}

		public void GetKnownCustomDataTypes(System.Collections.ObjectModel.Collection<Type> customDataTypes)
		{
			throw new NotImplementedException();
		}

		public object GetObjectToSerialize(object obj, Type targetType)
		{
			throw new NotImplementedException();
		}

		public Type GetReferencedTypeOnImport(string typeName, string typeNamespace, object customData)
		{
			throw new NotImplementedException();
		}

		public System.CodeDom.CodeTypeDeclaration ProcessImportedType(System.CodeDom.CodeTypeDeclaration typeDeclaration, System.CodeDom.CodeCompileUnit compileUnit)
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}

