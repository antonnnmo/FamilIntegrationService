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
			Schedule(() => sp.CreateScope().ServiceProvider.GetRequiredService<TableClearingJob>()).ToRunNow().AndEvery(3).Hours();
			Schedule(() => sp.CreateScope().ServiceProvider.GetRequiredService<CalculateJob>()).ToRunNow().AndEvery(1).Seconds();
		}
	}
}
