using FluentScheduler;
using ProcessingIntegrationService.Managers;

namespace ProcessingIntegrationService
{
	internal class CardClean : IJob
	{
		private readonly object _lock = new object();

		private bool _shuttingDown;

		public CardClean()
		{
		}

		public void Execute()
		{
			try
			{
				lock (_lock)
				{
					var synchronizer = new CardSynchronizer();
					synchronizer.CleanCardTempTable();
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