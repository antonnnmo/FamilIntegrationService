using FamilIntegrationCore.Models;
using Newtonsoft.Json;

namespace FamilIntegrationService.Models
{
	public class Card : BaseIntegrationObject
	{
		[JsonProperty]
		public string ActivationDate { get; set; }
		[JsonProperty]
		public string ErrorMessage { get; set; }
		[JsonProperty]
		public string CreatedOn { get; set; }
		[JsonProperty]
		public string Number { get; set; }
		[JsonProperty]
		public string ContactId { get; set; }
		[JsonProperty]
		public string CardStatus { get; set; }
		[JsonProperty]
		public string BlockingReason { get; set; }
		[JsonProperty]
		public string BlockedOn { get; set; }
		[JsonProperty]
		public bool IsMain { get; set; }

		public string ToJson()
		{
			return JsonConvert.SerializeObject(this);
		}

		public static Card FromJson(string json) => JsonConvert.DeserializeObject<Card>(json);
	}
}
