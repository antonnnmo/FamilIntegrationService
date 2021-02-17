using FluentScheduler;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FamilServiceMonitoringApp.Schedulle
{
	public class Scheduller : Registry
	{
		public Scheduller(IServiceProvider sp)
		{
			Logger.LogError("start scheduller");
			Schedule(() => sp.CreateScope().ServiceProvider.GetRequiredService<TableClearingJob>()).ToRunEvery(3).Hours();
			Schedule(() => sp.CreateScope().ServiceProvider.GetRequiredService<CalculateJob>()).ToRunNow().AndEvery(1).Seconds();
			Schedule(() => sp.CreateScope().ServiceProvider.GetRequiredService<ContactInfoJob>()).ToRunNow().AndEvery(1).Seconds();

			GlobalCacheReader.GetValue(GlobalCacheReader.CacheKeys.CacheInterval, out int minutes);
			Schedule(() => sp.CreateScope().ServiceProvider.GetRequiredService<CacheJob>()).ToRunEvery(minutes).Minutes();

		}
	}
}
