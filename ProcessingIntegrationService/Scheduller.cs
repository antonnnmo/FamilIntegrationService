using FamilIntegrationService.Providers;
using FluentScheduler;

namespace ProcessingIntegrationService
{
	public class Scheduller : Registry
	{
		public Scheduller()
		{
			GlobalCacheReader.GetValue(GlobalCacheReader.CacheKeys.CardSynchronizationPeriod, out int period);
			GlobalCacheReader.GetValue(GlobalCacheReader.CacheKeys.CardCleanPeriod, out int cleanPeriod);
			Schedule<CardJob>().ToRunNow().AndEvery(period).Minutes();
			Schedule<CardClean>().ToRunNow().AndEvery(cleanPeriod).Minutes();
		}
	}
}