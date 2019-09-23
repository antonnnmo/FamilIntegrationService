using Newtonsoft.Json;

namespace FamilIntegrationCore.Models
{
	public class IntegrationObjectRequest
	{
		[JsonProperty]
		public string TableName { get; set; }

		[JsonProperty]
		public string Objects { get; set; }

		public string ToJson()
		{
			return JsonConvert.SerializeObject(this);
		}
	}
}
