using Newtonsoft.Json;

namespace FamilIntegrationCore.Models
{
	public class ProductSubCategory : BaseIntegrationObject
	{
		[JsonProperty]
		public string Name { get; set; }
		[JsonProperty]
		public string Category { get; set; }
	}
}
