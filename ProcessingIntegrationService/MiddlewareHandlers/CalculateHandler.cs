﻿using FamilIntegrationService;
using Newtonsoft.Json.Linq;
using Npgsql;
using ProcessingIntegrationService;
using ProcessingIntegrationService.Coupons;
using ProcessingIntegrationService.Managers;
using ProcessingIntegrationService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using static ProcessingIntegrationService.Models.CouponResponse;

namespace LoyaltyMiddleware.MiddlewareHandlers
{
	public class CalculateHandler : IRequestHandler
	{
		public Dictionary<string, object> GetHandledResponse(Dictionary<string, object> requestData, Dictionary<string, object> responseData, Dictionary<string, object> additionalResponseData)
		{
			if (responseData.ContainsKey("success") && (bool)responseData["success"] == true)
			{
				RemoveCoupon(responseData, requestData);
				var couponTexts = GetCoupons(responseData);

				responseData.Add("coupons", couponTexts);

				if (responseData.ContainsKey("data") && responseData["data"] != null)
				{
					var data = (responseData["data"] as Newtonsoft.Json.Linq.JObject);
					if (data.ContainsKey("productDiscounts") && data["productDiscounts"] != null)
					{
						var removedDiscounts = new List<JToken>();
						var remainingDiscounts = new List<JToken>();
						var discounts = (data["productDiscounts"] as Newtonsoft.Json.Linq.JArray).ToList();
						discounts.ForEach(productDiscount =>
						{
							if (productDiscount["discounts"] != null)
							{
								var promotionDiscounts = (productDiscount["discounts"] as JArray).Where(d => d["type"].ToString() == "Promotion");

								if (promotionDiscounts.Count() > 0)
								{
									var maxDiscount = promotionDiscounts.Aggregate((d1, d2) => (decimal?)d1["discount"] > (decimal?)d2["discount"] ? d1 : d2);

									var removingDiscounts = (productDiscount["discounts"] as JArray).Where(d => d["type"].ToString() == "Promotion" && d != maxDiscount);
									if (removingDiscounts.Count() > 0)
									{
										removedDiscounts.AddRange(removingDiscounts);
									}

									var newDiscounts = (productDiscount["discounts"] as JArray).ToList();
									newDiscounts.RemoveAll(d => d["type"].ToString() == "Promotion" && d != maxDiscount);
									remainingDiscounts.AddRange(newDiscounts);
									(productDiscount["discounts"] as JArray).ReplaceAll(newDiscounts);
									//productDiscount["Discounts"] = newDiscounts as JToken;

									productDiscount["discount"] = newDiscounts.Sum(d => Convert.ToDecimal(d["discount"]));
								}
							}
						});

						var activatedPromotions = (data["activatedPromotions"] as Newtonsoft.Json.Linq.JArray).ToList();

						if (removedDiscounts.Count > 0)
						{
							var notExistingDiscounts = removedDiscounts.Where(d => remainingDiscounts.Count(d1 => d1["promotion"] != null && d1["promotion"]["name"] == d["promotion"]["name"]) == 0);
							activatedPromotions.RemoveAll(ap => notExistingDiscounts.Count(d => d["promotion"] != null && d["promotion"]["name"]?.ToString() == ap["name"]?.ToString()) > 0);
							data.Remove("activatedPromotions");
							data.Add("activatedPromotions", JToken.FromObject(activatedPromotions));
						}
					}
				}
				

				var price = 0d;
				var prices = new Dictionary<string, double>();
				using (var conn = new NpgsqlConnection(DBProvider.GetConnectionString()))
				{
					conn.Open();

					// Insert some data
					using (var cmd = new NpgsqlCommand())
					{
						cmd.Connection = conn;
						cmd.CommandText = String.Format(@"SELECT ""Price"", ""Code"" FROM public.""ProductRecommendedPrice"" WHERE ""Code"" IN({0})", String.Join(",", (requestData["products"] as JArray).Select(p => String.Format("'{0}'", p["productCode"]))));

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

		private List<CouponResponse> GetCoupons(Dictionary<string, object> responseData)
		{
			var couponTexts = new List<CouponResponse>();
			if (responseData["data"] != null && (responseData["data"] as JObject)["activatedPromotions"] != null)
			{
				var promotions = (responseData["data"] as JObject)["activatedPromotions"] as JArray;
				var promotionIds = promotions.Select(p => {
					if ((p as JObject)["id"] != null && Guid.TryParse((p as JObject)["id"].ToString(), out Guid id))
						return id;
					else return Guid.Empty;
				});

				if (promotionIds.Count() > 0) 
				{
					var coupons = CouponCache.Coupons.Where(c => c.IsActive && c.Promotions != null && c.Promotions.Any(p => promotionIds.Contains(p.Id))).ToList();

					if (coupons != null && coupons.Count() > 0) 
					{
						couponTexts = coupons.Select(coupon => 
							new CouponResponse() { Name = coupon.Name, Texts = coupon.Texts.Select(c => new CouponTextResponse() { Index = c.Order, Text = c.Text }).ToList() }
						).ToList();
					}
				}
			}

			return couponTexts;
		}

		private void RemoveCoupon(Dictionary<string, object> responseData, Dictionary<string, object> requestData)
		{
			if (!responseData.ContainsKey("data")) return;
			var couponProduct = (requestData["products"] as JArray).FirstOrDefault(p => (p as JObject)["productCode"] != null && (p as JObject)["productCode"].ToString() == "coupon") as JObject;
			if (couponProduct != null) 
			{
				var index = couponProduct["index"].ToString();
				if (responseData["data"] != null && (responseData["data"] as JObject)["productDiscounts"] != null)
				{
					var productDiscounts = (responseData["data"] as JObject)["productDiscounts"] as JArray;
					var couponDiscount = productDiscounts.FirstOrDefault(d => (d as JObject)["index"].ToString() == index);
					if (couponDiscount != null) productDiscounts.Remove(couponDiscount);
				}
			}
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
