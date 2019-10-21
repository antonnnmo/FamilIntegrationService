using FamilIntegrationService.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProcessingIntegrationService
{
	public class DBProvider
	{
		public static string GetConnectionString()
		{
			GlobalCacheReader.GetValue(GlobalCacheReader.CacheKeys.ConnectionString, out string connString);
			return connString;
		}
	}
}
