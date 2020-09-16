using FamilIntegrationService;
using Newtonsoft.Json.Linq;
using Npgsql;
using ProcessingIntegrationService;
using ProcessingIntegrationService.Managers;
using ProcessingIntegrationService.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LoyaltyMiddleware.MiddlewareHandlers
{
	public class CalculateHandler : IRequestHandler
	{
		public Dictionary<string, object> GetHandledResponse(Dictionary<string, object> requestData, Dictionary<string, object> responseData, Dictionary<string, object> additionalResponseData)
		{
			if (responseData.ContainsKey("success") && (bool)responseData["success"] == true)
			{
				var price = 0d;
				var prices = new Dictionary<string, double>();
				using (var conn = new NpgsqlConnection(DBProvider.GetConnectionString()))
				{
					conn.Open();

					// Insert some data
					using (var cmd = new NpgsqlCommand())
					{
						cmd.Connection = conn;
						cmd.CommandText = String.Format(@"SELECT ""Price"", ""Code"" FROM public.""ProductPrice"" WHERE ""Code"" IN({0})", String.Join(",", (requestData["products"] as JArray).Select(p => String.Format("'{0}'", p["productCode"]))));

						using (var reader = cmd.ExecuteReader())
						{
							while (reader.Read())
							{
								prices.Add(reader.GetString(1), reader.GetDouble(0));
							}
						}
					}
				}

				if (prices.Count > 0)
				{
					//price = prices.Sum(p => p.Value * Convert.ToDouble(request.Products.FirstOrDefault(pr => pr.ProductCode == p.Key).Quantity));
					price = (requestData["products"] as JArray).Sum(p => Convert.ToDouble(p["quantity"]) * prices.FirstOrDefault(pr => pr.Key == p["productCode"].ToString()).Value);

					var discounts = 0;


					if (responseData["data"] != null && (responseData["data"] as JObject)["productDiscounts"] != null)
					{
						discounts = Convert.ToInt32(((responseData["data"] as JObject)["productDiscounts"] as JArray).Sum(pd => pd["discounts"] == null ? 0 : (pd["discounts"] as JArray).Sum(d => (decimal)d["discount"])));
					}

					if (requestData.ContainsKey("amount"))
					{
						var diff = Convert.ToInt32(price - Convert.ToDouble(requestData["amount"]) + discounts);
						responseData.Add("benefitAmount", diff.ToString());

						var now = DateTime.UtcNow;
						responseData.Add("benefitFirst", AnswerTemplateCollection.CalculateResponseTemplatePrefix);
						responseData.Add("benefitSecond", $"{ GetDeclension(diff, "рубль", "рубля", "рублей")}. ");

						var rand = new Random();
						var prefixTemplate = AnswerTemplateCollection.Templates.OrderBy(t => rand.Next()).FirstOrDefault(t => t.IsFirstTextBlock && t.From <= diff && diff <= t.To && t.Start <= now && now <= t.End);
						if (prefixTemplate != null)
						{
							responseData["benefitSecond"] += $"{prefixTemplate.PrefixText} {Convert.ToInt32(prefixTemplate.Price != 0 ? diff / prefixTemplate.Price : 0)} {prefixTemplate.SuffixText} ";
						}

						var suffixTemplate = AnswerTemplateCollection.Templates.OrderBy(t => rand.Next()).FirstOrDefault(t => !t.IsFirstTextBlock && t.From <= diff && diff <= t.To && t.Start <= now && now <= t.End);
						if (suffixTemplate != null)
						{
							responseData["benefitSecond"] += $"{suffixTemplate.PrefixText} {Convert.ToInt32(suffixTemplate.Price != 0 ? diff / suffixTemplate.Price : 0)} {suffixTemplate.SuffixText}";
						}
					}
				}

				if (responseData.ContainsKey("ActivePromocodes")) responseData.Remove("ActivePromocodes");
				var client = (requestData["client"] as JObject);
				responseData.Add("ActivePromocodes", Promocode.GetActivePromocodes(client["mobilePhone"]?.ToString(), client["cardNumber"]?.ToString()));
			}

			if (additionalResponseData != null) 
			{
				responseData = new Dictionary<string, object>[] { responseData, additionalResponseData }.SelectMany(dict => dict)
						 .ToDictionary(pair => pair.Key, pair => pair.Value);
			}
			return responseData;
		}

		private static string GetDeclension(int number, string nominativ, string genetiv, string plural)
		{
			number = number % 100;
			if (number >= 11 && number <= 19)
			{
				return plural;
			}

			var i = number % 10;
			switch (i)
			{
				case 1:
					return nominativ;
				case 2:
				case 3:
				case 4:
					return genetiv;
				default:
					return plural;
			}

		}
	}
}
