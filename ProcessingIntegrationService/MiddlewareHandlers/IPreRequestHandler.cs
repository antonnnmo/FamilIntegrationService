using System.Collections.Generic;

namespace RedmondLoyaltyMiddleware.MiddlewareHandlers
{
	interface IPreRequestHandler
	{
		PreHandlerResult GetHandledRequest(Dictionary<string, object> requestData);
	}
}
