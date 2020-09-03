using Newtonsoft.Json.Linq;
using ProcessingIntegrationService;
using ProcessingIntegrationService.Managers;
using System.Collections.Generic;

namespace LoyaltyMiddleware.MiddlewareHandlers
{
	internal class ConfirmHandler : IRequestHandler
	{
		public static string noNamePhone = "70000000000";
		public static string sberbankPhone = "70000000001";

		public ConfirmHandler()
		{
		}

		public Dictionary<string, object> GetHandledResponse(Dictionary<string, object> requestData, Dictionary<string, object> responseData, Dictionary<string, object> additionalResponseData)
		{
			//todo: confirm middlewarecode
			if (requestData["client"] != null && (requestData["client"] as JObject)["mobilePhone"].ToString() == noNamePhone)
			{
				if (responseData.ContainsKey("client")) responseData.Remove("client");
				responseData.Add("client", new ResponseClient() { Name = "NoName" });
			}
			else if (requestData["client"] != null && (requestData["client"] as JObject)["mobilePhone"].ToString() == sberbankPhone)
			{
				if (responseData.ContainsKey("client")) responseData.Remove("client");
				responseData.Add("client", new ResponseClient() { Name = "Sberbank" });
			}

			if (responseData.ContainsKey("success") && (bool)responseData["success"] == true)
			{
				if (responseData.ContainsKey("ActivePromocodes")) responseData.Remove("ActivePromocodes");
				var client = (requestData["client"] as JObject);
				responseData.Add("ActivePromocodes", Promocode.GetActivePromocodes(client["mobilePhone"]?.ToString(), client["cardNumber"]?.ToString()));
			}

			return responseData;
		}
	}
}