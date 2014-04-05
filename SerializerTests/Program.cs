using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Diagnostics;

namespace SerializerTests
{
	class MainClass
	{
		public static void Main(string[] args)
		{
			IDictionary<string, object> data = new Dictionary<string, object>();
			data.Add("d1", (int)3);
			data.Add("d2", 6.8f);
			data.Add("d3", DateTime.Now);
			data.Add("d4", new Guid());
			data.Add("d5", new StackTrace());

			byte[] dataBytes;
			BinaryFormatter bf = new BinaryFormatter(null, new StreamingContext(StreamingContextStates.CrossAppDomain));
			using (MemoryStream ms = new MemoryStream())		//(Console.OpenStandardOutput()))
			{
				bf.Serialize(ms, data);
				dataBytes = ms.GetBuffer();
			}

			Console.WriteLine("dataBytes is {0} bytes\n", dataBytes.Length);

			using (MemoryStream ms = new MemoryStream(dataBytes))
			{
				IDictionary<string, object> dsData = (IDictionary<string, object>)bf.Deserialize(ms);
				foreach (KeyValuePair<string, object> dataPair in dsData)
					Console.WriteLine("{0}={1}", dataPair.Key, dataPair.Value.ToString());
			}
		}
	}
}
