using FamilIntegrationCore.Models;
using Newtonsoft.Json;

namespace FamilIntegrationService.Models
{
	public class Shop : BaseIntegrationObject
	{
		[JsonProperty]
		public string ErrorMessage { get; set; }
		[JsonProperty]
		public string CreatedOn { get; set; }
		[JsonProperty]
		public string Name { get; set; }
		[JsonProperty]
		public string Code { get; set; }
		[JsonProperty]
		public string Cluster { get; set; }
		[JsonProperty]
		public string City { get; set; }
		[JsonProperty]
		public string Description { get; set; }

		public string ToJson()
		{
			return JsonConvert.SerializeObject(this);
		}

		public static Shop FromJson(string json) => JsonConvert.DeserializeObject<Shop>(json);
	}
}
