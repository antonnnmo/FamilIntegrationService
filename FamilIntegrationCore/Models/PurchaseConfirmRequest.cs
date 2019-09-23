namespace FamilIntegrationCore.Models
{
	using System;
	using System.Collections.Generic;
	using Newtonsoft.Json;

	public partial class PurchaseConfirmRequest
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
		public IEnumerable<PCProduct> Products { get; set; }

		[JsonProperty("payments")]
		public IEnumerable<Payment> Payments { get; set; }

		[JsonProperty("paymentForm")]
		public string PaymentForm { get; set; }

		[JsonProperty("isTesting")]
		public bool IsTesting { get; set; }

		[JsonProperty("number")]
		public string Number { get; set; }

		[JsonProperty("cashdeskCode")]
		public string CashdeskCode { get; set; }

		[JsonProperty("promoCodes")]
		public string[] PromoCodes { get; set; }

		[JsonProperty("customFields")]
		public CustomFields CustomFields { get; set; }

		public string ToJson()
		{
			return JsonConvert.SerializeObject(this);
		}
	}

	public partial class Client
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

	public partial class CustomFields
	{
	}

	public partial class Payment
	{
		[JsonProperty("type")]
		public string Type { get; set; }

		[JsonProperty("amount")]
		public decimal Amount { get; set; }
	}

	public partial class PCProduct
	{
		[JsonProperty("index")]
		public int Index { get; set; }

		[JsonProperty("productCode")]
		public string ProductCode { get; set; }

		[JsonProperty("price")]
		public decimal Price { get; set; }

		[JsonProperty("quantity")]
		public int Quantity { get; set; }

		[JsonProperty("amount")]
		public decimal Amount { get; set; }
	}
}
