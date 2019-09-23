using Newtonsoft.Json;

namespace FamilIntegrationCore.Models
{
	public class BrandType : BaseIntegrationObject
	{
		[JsonProperty]
		public string Name { get; set; }
		[JsonProperty]
		public string Subcategory { get; set; }
	}
}
