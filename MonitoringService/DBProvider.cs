using FamilIntegrationService.Providers;

namespace MonitoringHelperService
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
