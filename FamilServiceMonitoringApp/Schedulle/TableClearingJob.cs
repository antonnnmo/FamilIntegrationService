using FamilServiceMonitoringApp.DB;
using FluentScheduler;
using System;
using System.Linq;

namespace FamilServiceMonitoringApp.Schedulle
{
	internal class TableClearingJob : IJob
	{
		private readonly object _lock = new object();
		private readonly ServiceDBContext _dbContext;
		private bool _shuttingDown;

		public TableClearingJob(ServiceDBContext dbContext)
		{
			_dbContext = dbContext;
		}

		public void Execute()
		{
			try
			{
				lock (_lock)
				{
					var oldEntities = _dbContext.Events.Where(e => e.Time < DateTime.UtcNow.AddMonths(-3));
					if (oldEntities != null && oldEntities.Count() > 0) 
					{
						_dbContext.Events.RemoveRange(oldEntities);
					}
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
