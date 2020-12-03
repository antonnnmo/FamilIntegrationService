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
			public static string BPMCookie { get { return "BPMCookie"; } }
			public static string BPMLogin { get { return "BPMLogin"; } }
			public static string BPMPassword { get { return "BPMPassword"; } }
			public static string BPMUri { get { return "BPMUri"; } }
			public static string BPMCSRF { get { return "BPMCSRF"; } }
			public static string ProcessingUri { get { return "ProcessingUri"; } }
			public static string PersonalAreaUri { get { return "PersonalAreaUri"; } }
			public static string ConnectionString { get { return "ConnectionString"; } }
            public static string ProcessingSecret { get { return "ProcessingSecret"; } }
            public static string PersonalAreaLogin { get { return "PersonalAreaLogin"; } }
            public static string PersonalAreaPasword { get { return "PersonalAreaPasword"; } }
            public static string PersonalAreaToken { get { return "PersonalAreaToken"; } }
            public static string CardSynchronizationPeriod { get { return "CardSynchronizationPeriod"; } }
            public static string CardCleanPeriod { get { return "CardCleanPeriod"; } }
        }

		internal static void GetValue(object personalAreaLogin, out string login)
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
