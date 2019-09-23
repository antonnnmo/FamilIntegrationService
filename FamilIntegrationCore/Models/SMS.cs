using FamilIntegrationCore.Models;
using Newtonsoft.Json;

namespace FamilIntegrationService.Models
{
	public class SMS : BaseIntegrationObject
	{
		[JsonProperty]
		public string ErrorMessage { get; set; }
		[JsonProperty]
		public string CreatedOn { get; set; }
		[JsonProperty]
		public string ContactId { get; set; }
		[JsonProperty]
		public string ContactPhone { get; set; }
		[JsonProperty]
		public string Text { get; set; }
		[JsonProperty]
		public string MessageStatus { get; set; }
		[JsonProperty]
		public string SentDate { get; set; }

		public string ToJson()
		{
			return JsonConvert.SerializeObject(this);
		}

		public static SMS FromJson(string json) => JsonConvert.DeserializeObject<SMS>(json);
	}
}
