using FamilIntegrationService.Providers;

namespace LoyaltyMiddleware.Loyalty
{
	public class PersonalAreaManager: ProcessingManager
	{
		public PersonalAreaManager()
		{
			GlobalCacheReader.GetValue(GlobalCacheReader.CacheKeys.PersonalAreaUri, out _uri);
		}
	}
}
