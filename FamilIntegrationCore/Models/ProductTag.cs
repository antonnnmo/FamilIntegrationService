using Newtonsoft.Json;

namespace FamilIntegrationCore.Models
{
	public class ProductTag : BaseIntegrationObject
	{
		[JsonProperty]
		public string Name { get; set; }
		[JsonProperty]
		public string ProductId { get; set; }
	}
}
