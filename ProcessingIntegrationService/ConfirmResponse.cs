using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ProcessingIntegrationService
{
	public class ConfirmResponse
	{
		[JsonProperty("success")]
		public bool Success { get; set; }

		[JsonProperty("error")]
		public object Error { get; set; }

		[JsonProperty("data")]
		public Data Data { get; set; }

		[JsonProperty("benefitAmount")]
		public string BenefitAmount { get; set; }

		[JsonProperty("benefitSecond")]
		public string BenefitSecond { get; set; }

		[JsonProperty("benefitFirst")]
		public string BenefitFirst { get; set; }

		[JsonProperty("client")]
		public ResponseClient Client { get; set; }

		public static ConfirmResponse FromJson(string json) => JsonConvert.DeserializeObject<ConfirmResponse>(json, ConfirmResponseConverter.Settings);

		public string ToJson()
		{
			return JsonConvert.SerializeObject(this, ConfirmResponseConverter.Settings);
		}
	}

	public class ResponseClient
	{
		[JsonProperty("name")]
		public string Name { get; set; }
	}

	public class Data
	{
		[JsonProperty("chargedBonuses")]
		public ChargedBonus[] ChargedBonuses { get; set; }
	}

	public class ChargedBonus
	{
		[JsonProperty("amount")]
		public decimal Amount { get; set; }

		[JsonProperty("promotion")]
		public Promotion Promotion { get; set; }
	}

	public class Promotion
	{
		[JsonProperty("name")]
		public string Name { get; set; }

		[JsonProperty("description")]
		public string Description { get; set; }
	}

	internal static class ConfirmResponseConverter
	{
		public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
		{
			MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
			DateParseHandling = DateParseHandling.None,
			Converters =
			{
				new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
			},
		};
	}
}
