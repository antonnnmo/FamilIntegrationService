using Newtonsoft.Json;

namespace FamilIntegrationCore.Models
{
	public class ProductInPurchase: BaseIntegrationObject
	{
		[JsonProperty]
		public decimal Price { get; set; }
		[JsonProperty]
		public int Quantity { get; set; }
		[JsonProperty]
		public decimal Amount { get; set; }
		[JsonProperty]
		public string ProductCode { get; set; }
		[JsonProperty]
		public string ProductCode { get; set; }
	}
}
