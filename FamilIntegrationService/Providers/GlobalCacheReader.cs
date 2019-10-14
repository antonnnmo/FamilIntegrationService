using Microsoft.Extensions.Caching.Memory;
using System;

namespace FamilIntegrationService.Providers
{
	public static class GlobalCacheReader
	{
		public static MemoryCache Cache { get; set; }

		static GlobalCacheReader()
		{
			Cache = new MemoryCache(new MemoryCacheOptions() { });
		}

		public static class CacheKeys
		{
			public static string SqlConnectionString { get { return "SqlConnectionString"; } }
			public static string CRMSqlConnectionString { get { return "CRMSqlConnectionString"; } }
			public static string BPMCookie { get { return "BPMCookie"; } }
			public static string BPMLogin { get { return "BPMLogin"; } }
			public static string BPMPassword { get { return "BPMPassword"; } }
			public static string BPMUri { get { return "BPMUri"; } }
			public static string BPMCSRF { get { return "BPMCSRF"; } }
			public static string ProcessingUri { get { return "ProcessingUri"; } }
			public static string ProcessingLogin { get { return "ProcessingLogin"; } }
			public static string ProcessingPasword { get { return "ProcessingPasword"; } }
			public static string ProcessingToken { get { return "ProcessingToken"; } }
            public static string PackSize { get { return "PackSize"; } }
            public static string ThreadCount { get { return "ThreadCount"; } }
            public static string CrmRequestTimeout { get { return "CrmRequestTimeout"; } }
        }

		internal static void GetValue(object processingLogin, out string login)
		{
			throw new NotImplementedException();
		}

		public static bool GetValue<T>(string key, out T value)
		{
			return Cache.TryGetValue(key, out value);
		}

		public static void SetValue<T>(string key, T value)
		{
			GlobalCacheReader.Cache.Set(key, value);
		}

		public static void SetTemporaryValue<T>(string key, T value, TimeSpan lifeTime)
		{
			GlobalCacheReader.Cache.Set(key, value, lifeTime);
		}
	}
}
