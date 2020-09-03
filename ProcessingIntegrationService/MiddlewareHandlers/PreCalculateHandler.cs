using System.Collections.Generic;

namespace RedmondLoyaltyMiddleware.MiddlewareHandlers
{
	public class PreCalculateHandler : IPreRequestHandler
	{
		public PreHandlerResult GetHandledRequest(Dictionary<string, object> requestData)
		{
			var result = new PreHandlerResult();
			
			result.Request = requestData;
			return result;
		}
	}
}
