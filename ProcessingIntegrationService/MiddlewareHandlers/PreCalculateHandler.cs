using System.Collections.Generic;

namespace RedmondLoyaltyMiddleware.MiddlewareHandlers
{
	public class PreCalculateHandler : IPreRequestHandler
	{
		public PreHandlerResult GetHandledRequest(Dictionary<string, object> requestData)
		{
			var result = new PreHandlerResult();

			if (requestData.ContainsKey("useMaxDiscount")) requestData.Remove("useMaxDiscount");
			requestData.Add("useMaxDiscount", true);

			result.Request = requestData;
			return result;
		}
	}
}
