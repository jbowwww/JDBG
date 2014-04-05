using System;

namespace TraceService
{
	public enum MessageLevel
	{
		Fatal = -64,
		Critical = -32,
		Error = -16,
		Warning = -8,
		Information = 0,
		Verbose = 8,
		Debug = 16,
		User = 512
	}
}

