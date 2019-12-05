using Newtonsoft.Json;

namespace FamilIntegrationCore.Models
{
	public class ContactBalance
	{
		[JsonProperty]
		public string ERPId { get; set; }
		[JsonProperty]
		public string BonusType { get; set; }
		[JsonProperty]
		public decimal Balance { get; set; }
	}
}
