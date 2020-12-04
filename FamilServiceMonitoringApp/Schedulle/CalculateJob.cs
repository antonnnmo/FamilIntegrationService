using FamilServiceMonitoringApp.DB;
using FamilServiceMonitoringApp.Processing;
using FluentScheduler;
using System;

namespace FamilServiceMonitoringApp.Schedulle
{
	internal class CalculateJob : IJob
	{
		private readonly object _lock = new object();
		private readonly ServiceDBContext _dbContext;
		private bool _shuttingDown;

		public CalculateJob(ServiceDBContext dbContext)
		{
			_dbContext = dbContext;
		}

		public void Execute()
		{
			try
			{
				lock (_lock)
				{
					var currentTime = DateTime.UtcNow;
					var result = CalculateProcessor.Calculate();
					_dbContext.Events.Add(new Event()
					{
						EventName = "Calculate",
						Id = Guid.NewGuid(),
						Time = currentTime,
						Timeout = Convert.ToInt32(result.Time),
						IsError = result.IsError
					});

					_dbContext.SaveChanges();
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