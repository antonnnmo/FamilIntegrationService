using Newtonsoft.Json;

namespace FamilIntegrationCore.Models
{
	public class PaymentInPurchase : BaseIntegrationObject
	{
		[JsonProperty]
		public string Type { get; set; }
		[JsonProperty]
		public decimal Amount { get; set; }
		[JsonProperty]
		public string PurchaseId { get; set; }
	}
}
