using System;
using System.Collections.Generic;

using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ProcessingIntegrationService
{
	public class PurchaseConfirmRequest
	{
		[JsonProperty("id")]
		public string Id { get; set; }

		[JsonProperty("client")]
		public Client Client { get; set; }

		[JsonProperty("date")]
		public DateTimeOffset Date { get; set; }

		[JsonProperty("shopCode")]
		public string ShopCode { get; set; }

		[JsonProperty("products")]
		public Product[] Products { get; set; }

		[JsonProperty("payments")]
		public Payment[] Payments { get; set; }

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

		[JsonProperty("isTesting")]
		public bool IsTesting { get; set; }

		[JsonProperty("number")]
		public string Number { get; set; }

		[JsonProperty("amount")]
		public decimal Amount { get; set; }

		[JsonProperty("cashdeskCode")]
		public string CashdeskCode { get; set; }

		[JsonProperty("promoCodes")]
		public string[] PromoCodes { get; set; }

		[JsonProperty("customFields")]
		public Dictionary<string, string> CustomFields { get; set; }

		public string ToJson()
		{
			return JsonConvert.SerializeObject(this, Converter.Settings);
		}
	}

	public class Client
	{
		[JsonProperty("mobilePhone")]
		public string MobilePhone { get; set; }

		[JsonProperty("cardNumber")]
		public string CardNumber { get; set; }

		[JsonProperty("id")]
		public string Id { get; set; }

		[JsonProperty("brandCode")]
		public string BrandCode { get; set; }
	}

	public class Payment
	{
		[JsonProperty("type")]
		public string Type { get; set; }

		[JsonProperty("amount")]
		public decimal Amount { get; set; }
	}

	public class Product
	{
		[JsonProperty("index")]
		public long Index { get; set; }

		[JsonProperty("productCode")]
		public string ProductCode { get; set; }

		[JsonProperty("price")]
		public decimal Price { get; set; }

		[JsonProperty("quantity")]
		public decimal Quantity { get; set; }

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
}

