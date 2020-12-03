using FluentScheduler;
using ProcessingIntegrationService.Managers;

namespace ProcessingIntegrationService
{
	internal class CardJob : IJob
	{
		private readonly object _lock = new object();

		private bool _shuttingDown;

		public CardJob()
		{
		}

		public void Execute()
		{
			try
			{
				lock (_lock)
				{
					var synchronizer = new CardSynchronizer();
					synchronizer.SynchronizeCardWithCS();
					synchronizer.SynchronizeCardWithBPM();
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