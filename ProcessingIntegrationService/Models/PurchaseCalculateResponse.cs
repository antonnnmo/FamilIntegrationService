using System;
using System.Collections.Generic;

using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ProcessingIntegrationService.Models
{
	public class PurchaseCalculateResponse
	{
		[JsonProperty("error")]
		public object Error { get; set; }

		[JsonProperty("data")]
		public Data Data { get; set; }

		[JsonProperty("activePromocodes")]
		public List<ActivePromocode> ActivePromocodes { get; set; }

		[JsonProperty("benefitAmount")]
		public string BenefitAmount { get; set; }

		[JsonProperty("benefitSecond")]
		public string BenefitSecond { get; set; }

		[JsonProperty("benefitFirst")]
		public string BenefitFirst { get; set; }

		[JsonProperty("success")]
		public bool Success { get; set; }

		public static PurchaseCalculateResponse FromJson(string json) => JsonConvert.DeserializeObject<PurchaseCalculateResponse>(json, CalculateResponseConverter.Settings);
	}

	public class BonusInfo
	{
	}

	public class BonusInfoMain
	{
		[JsonProperty("balance")]
		[JsonConverter(typeof(DecimalFormatConverter))]
		public decimal Balance { get; set; }

		[JsonProperty("totalBalance")]
		[JsonConverter(typeof(DecimalFormatConverter))]
		public decimal TotalBalance { get; set; }

		[JsonProperty("availableAmount")]
		[JsonConverter(typeof(DecimalFormatConverter))]
		public decimal AvailableAmount { get; set; }

		[JsonProperty("minAvailableAmount")]
		[JsonConverter(typeof(DecimalFormatConverter))]
		public decimal MinAvailableAmount { get; set; }
	}

	public class Data
	{
		[JsonProperty("client")]
		public CalculateResponseClient Client { get; set; }

		[JsonProperty("productDiscounts")]
		public ProductDiscount[] ProductDiscounts { get; set; }

		[JsonProperty("activatedPromotions")]
		public List<Promotion> ActivatedPromotions { get; set; }

		[JsonProperty("bonusBalance")]
		[JsonConverter(typeof(DecimalFormatConverter))]
		public decimal BonusBalance { get; set; }

		[JsonProperty("totalBonusBalance")]
		[JsonConverter(typeof(DecimalFormatConverter))]
		public decimal TotalBonusBalance { get; set; }

		[JsonProperty("availableBonusPercent")]
		[JsonConverter(typeof(DecimalFormatConverter))]
		public decimal AvailableBonusPercent { get; set; }

		[JsonProperty("availableBonusAmount")]
		[JsonConverter(typeof(DecimalFormatConverter))]
		public decimal AvailableBonusAmount { get; set; }

		[JsonProperty("minAvailableBonusAmount")]
		[JsonConverter(typeof(DecimalFormatConverter))]
		public decimal MinAvailableBonusAmount { get; set; }

		[JsonProperty("bonusInfo")]
		public Dictionary<string, BonusInfoMain> BonusInfo { get; set; }
	}

	public class Promotion
	{
		[JsonProperty("name")]
		public string Name { get; set; }

		[JsonProperty("description")]
		public string Description { get; set; }
	}

	public class CalculateResponseClient
	{
		[JsonProperty("name")]
		public string Name { get; set; }

		[JsonProperty("mobilePhone")]
		public string MobilePhone { get; set; }

		[JsonProperty("authCode")]
		public string AuthCode { get; set; }

		[JsonProperty("brand")]
		public Brand Brand { get; set; }

		[JsonProperty("cards")]
		public Card[] Cards { get; set; }
	}

	public class Brand
	{
		[JsonProperty("code")]
		public string Code { get; set; }
	}

	public class Card
	{
		[JsonProperty("number")]
		public string Number { get; set; }

		[JsonProperty("state")]
		public string State { get; set; }

		[JsonProperty("isMain")]
		public bool IsMain { get; set; }
	}

	public class ProductDiscount
	{
		[JsonProperty("index")]
		public long Index { get; set; }

		[JsonProperty("discounts")]
		public Discount[] Discounts { get; set; }

		[JsonProperty("discount")]
		[JsonConverter(typeof(DecimalFormatConverter))]
		public decimal Discount { get; set; }
	}

	public class Discount
	{
		[JsonProperty("promotion", NullValueHandling = NullValueHandling.Ignore)]
		public Promotion Promotion { get; set; }

		[JsonProperty("type")]
		public string Type { get; set; }

		[JsonProperty("discount")]
		[JsonConverter(typeof(DecimalFormatConverter))]
		public decimal DiscountDiscount { get; set; }
	}


	public static class Serialize
	{
		public static string ToJson(this PurchaseCalculateResponse self) => JsonConvert.SerializeObject(self, CalculateResponseConverter.Settings);
	}

	internal static class CalculateResponseConverter
	{
		public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
		{
			MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
			DateParseHandling = DateParseHandling.None,
			Converters =
			{
				new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal },
				new DecimalFormatConverter()
			},
		};
	}

	public class DecimalFormatConverter : JsonConverter
	{
		public override bool CanConvert(Type objectType)
		{
			return (objectType == typeof(decimal));
		}

		public override void WriteJson(JsonWriter writer, object value,
									   JsonSerializer serializer)
		{
			writer.WriteRawValue(string.Format("{0:F0}", Math.Floor((decimal)value)) + ".00");
		}

		public override bool CanRead
		{
			get { return false; }
		}

		public override object ReadJson(JsonReader reader, Type objectType,
									 object existingValue, JsonSerializer serializer)
		{
			throw new NotImplementedException();
		}
	}

	internal class CalculateResponseParseStringConverter : JsonConverter
	{
		public override bool CanConvert(Type t) => t == typeof(long) || t == typeof(long?);

		public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
		{
			if (reader.TokenType == JsonToken.Null) return null;
			var value = serializer.Deserialize<string>(reader);
			long l;
			if (Int64.TryParse(value, out l))
			{
				return l;
			}
			throw new Exception("Cannot unmarshal type long. " + value.ToString());
		}

		public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
		{
			if (untypedValue == null)
			{
				serializer.Serialize(writer, null);
				return;
			}
			var value = (long)untypedValue;
			serializer.Serialize(writer, value.ToString());
			return;
		}

		public static readonly CalculateResponseParseStringConverter Singleton = new CalculateResponseParseStringConverter();
	}
}
