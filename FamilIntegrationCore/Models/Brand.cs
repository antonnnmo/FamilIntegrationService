using Newtonsoft.Json;

namespace FamilIntegrationCore.Models
{
	public class Brand : BaseIntegrationObject
	{
		[JsonProperty]
		public string Name { get; set; }
		[JsonProperty]
		public string BrandType { get; set; }
	}
}
