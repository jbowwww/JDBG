using System;

namespace TraceService
{
	public abstract class Disposable : IDisposable
	{
		#region IDisposable implementation
		protected bool _disposed = false;

		~Disposable()
		{
			Dispose(false);
		}

		/// <summary>
		/// Releases all resource used by the <see cref="TraceService.Listener"/> object.
		/// </summary>
		/// <remarks>Call <see cref="Dispose"/> when you are finished using the <see cref="TraceService.Listener"/>. The
		/// <see cref="Dispose"/> method leaves the <see cref="TraceService.Listener"/> in an unusable state. After calling
		/// <see cref="Dispose"/>, you must release all references to the <see cref="TraceService.Listener"/> so the garbage
		/// collector can reclaim the memory that the <see cref="TraceService.Listener"/> was occupying.</remarks>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Dispose the specified disposing.
		/// </summary>
		/// <param name="disposing">If set to <c>true</c> disposing.</param>
		protected virtual void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
					DisposeManaged();		// Dispose managed resources
				DisposeUnmanaged();		// Dispose unmanaged resources
				_disposed = true;
			}
		}

		/// <summary>
		/// Disposes the managed resources
		/// </summary>
		protected virtual void DisposeManaged()
		{

		}

		/// <summary>
		/// Disposes the unmanaged resources
		/// </summary>
		protected virtual void DisposeUnmanaged()
		{

		}
		#endregion
	}
}

