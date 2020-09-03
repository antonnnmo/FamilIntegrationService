using System.Collections.Generic;

namespace RedmondLoyaltyMiddleware.MiddlewareHandlers
{
	public class PreHandlerResult
	{
		public PreHandlerResult()
		{
			Request = new Dictionary<string, object>();
			AdditionalResponseData = new Dictionary<string, object>();
		}
		public Dictionary<string, object> Request { get; set; }
		public Dictionary<string, object> AdditionalResponseData { get; set; }
	}
}
