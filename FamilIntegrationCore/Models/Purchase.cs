using System;
using System.Collections.Generic;
using FamilIntegrationCore.Models;
using Newtonsoft.Json;

namespace FamilIntegrationService.Models
{
	public class Purchase : BaseIntegrationObject
	{
		[JsonProperty]
		public string ErrorMessage { get; set; }
		[JsonProperty]
		public string CreatedOn { get; set; }
		[JsonProperty]
		public string Number { get; set; }
		[JsonProperty]
		public string ContactId { get; set; }
		[JsonProperty]
		public string Phone { get; set; }
		[JsonProperty]
		public string CardNumber { get; set; }
		[JsonProperty]
		public string ShopCode { get; set; }
		[JsonProperty]
		public string CashDeskCode { get; set; }
		[JsonProperty]
		public string PaymentForm { get; set; }
		[JsonProperty]
		public decimal Amount { get; set; }
		[JsonProperty]
		public string PurchaseDate { get; set; }
		
		[JsonIgnore]
		public DateTime PDate { get; set; }

		[JsonIgnore]
		public List<ProductInPurchase> ProductsInPurchase { get; set; }

		[JsonIgnore]
		public List<PaymentInPurchase> PaymentsInPurchase { get; set; }

		public string ToJson()
		{
			return JsonConvert.SerializeObject(this);
		}

		public static Purchase FromJson(string json) => JsonConvert.DeserializeObject<Purchase>(json);
	}
}
