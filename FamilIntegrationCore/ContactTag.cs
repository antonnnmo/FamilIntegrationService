using Newtonsoft.Json;

namespace FamilIntegrationCore.Models
{
	public class ContactTag : BaseIntegrationObject
	{
		[JsonProperty]
		public string Name { get; set; }
		[JsonProperty]
		public string ContactId { get; set; }
	}
}
