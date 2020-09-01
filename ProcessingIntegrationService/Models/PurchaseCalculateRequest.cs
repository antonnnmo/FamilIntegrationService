using System;
using System.Collections.Generic;

using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ProcessingIntegrationService.Models
{
	public class PurchaseCalculateRequest
	{
		[JsonProperty("client")]
		public Client Client { get; set; }

		[JsonProperty("date")]
		public DateTimeOffset Date { get; set; }

		[JsonProperty("shopCode")]
		public string ShopCode { get; set; }

		[JsonProperty("products")]
		public Product[] Products { get; set; }

		[JsonProperty("bonusAmountToPay")]
		public double? BonusAmountToPay { get; set; }

		[JsonProperty("promoCodes")]
		public string[] PromoCodes { get; set; }

		[JsonProperty("amount")]
		public long Amount { get; set; }

		private string _paymentForm;

		[JsonProperty("paymentForm")]
		public string PaymentForm
		{
			get
			{
				if (String.IsNullOrEmpty(_paymentForm)) return "Fullpayment";
				else return _paymentForm;
			}
			set
			{
				_paymentForm = value;
			}
		}

		public static PurchaseCalculateRequest FromJson(string json) => JsonConvert.DeserializeObject<PurchaseCalculateRequest>(json, Converter.Settings);

		public string ToJson()
		{
			return JsonConvert.SerializeObject(this, Converter.Settings);
		}
	}

	public class Client
	{
		[JsonProperty("mobilePhone")]
		public string MobilePhone { get; set; }

		[JsonProperty("id")]
		public string Id { get; set; }

		[JsonProperty("cardNumber")]
		public string CardNumber { get; set; }

		[JsonProperty("brandCode")]
		public string BrandCode { get; set; }
	}

	public class Product
	{
		[JsonProperty("index")]
		public long Index { get; set; }

		[JsonProperty("price")]
		public decimal Price { get; set; }

		[JsonProperty("productCode")]
		public string ProductCode { get; set; }

		[JsonProperty("quantity")]
		public long Quantity { get; set; }

		[JsonProperty("amount")]
		public decimal Amount { get; set; }
	}

	internal static class Converter
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

	internal class ParseStringConverter : JsonConverter
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
			throw new Exception("Cannot unmarshal type long");
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

		public static readonly ParseStringConverter Singleton = new ParseStringConverter();
	}
}
