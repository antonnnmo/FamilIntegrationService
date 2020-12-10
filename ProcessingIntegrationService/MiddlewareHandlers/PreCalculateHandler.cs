using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace RedmondLoyaltyMiddleware.MiddlewareHandlers
{
	public class PreCalculateHandler : IPreRequestHandler
	{
		public PreHandlerResult GetHandledRequest(Dictionary<string, object> requestData)
		{
			var result = new PreHandlerResult();

			if (requestData.ContainsKey("useMaxDiscount")) requestData.Remove("useMaxDiscount");
			requestData.Add("useMaxDiscount", false);

			if (requestData.ContainsKey("products"))
			{
				var products = requestData["products"] as JArray;
				products.Add(
					JToken.FromObject(new ProductDto() 
					{
						Index = products.Count + 1,
						Price = 0,
						ProductCode = "coupon",
						Quantity = 1,
						Amount = 0
					})
				);
			}

			result.Request = requestData;
			return result;
		}
	}

	public class ProductDto 
	{
		[JsonProperty("index")]
		public int Index { get; set; }
		[JsonProperty("price")]
		public decimal Price { get; set; }
		[JsonProperty("productCode")]
		public string ProductCode { get; set; }
		[JsonProperty("quantity")]
		public int Quantity { get; set; }
		[JsonProperty("amount")]
		public decimal Amount { get; set; }
	}
}
