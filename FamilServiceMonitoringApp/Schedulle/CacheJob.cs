using FamilServiceMonitoringApp.DB;
using FluentScheduler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FamilServiceMonitoringApp.Schedulle
{
    internal class CacheJob : IJob
    {
		private readonly object _lock = new object();

		private bool _shuttingDown;

		public CacheJob()
		{

		}

		public void Execute()
		{
			try
			{
				lock (_lock)
				{
					SimpleMemoryCache.Instance.Clear();
				}
			}
			finally
			{
			}
		}

		public void Stop(bool immediate)
		{
			lock (_lock)
			{
				_shuttingDown = true;
			}

		}
	}
}
