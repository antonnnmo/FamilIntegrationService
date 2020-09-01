﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FamilIntegrationCore.Models;
using FamilIntegrationService;
using FamilIntegrationService.Providers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Npgsql;
using ProcessingIntegrationService.Managers;
using ProcessingIntegrationService.Models;

namespace ProcessingIntegrationService.Controllers
{
	[Route("purchase")]
	[ApiController]
	public class PurchaseController : ControllerBase
	{
		public static string noNamePhone = "70000000000";
		public static string sberbankPhone = "70000000001";
        private string _processingUrl = "";
        private string _processingSecret = "";

        public PurchaseController()
        {
            GlobalCacheReader.GetValue(GlobalCacheReader.CacheKeys.ProcessingUri, out _processingUrl);
            GlobalCacheReader.GetValue(GlobalCacheReader.CacheKeys.ProcessingSecret, out _processingSecret);
        }

		[HttpPost("calculate")]
		public ActionResult Calculate([FromBody]PurchaseCalculateRequest request)
		{
			var authHeader = HttpContext.Request.Headers["Authorization"];
			if (authHeader.Count == 0) return Unauthorized();
			Logger.LogInfo("started processing request", "");
			var res = PRRequest("calculate", request.ToJson(), authHeader);
			Logger.LogInfo("done processing request", "");
			if (res.IsSuccess)
			{
				var responseObj = PurchaseCalculateResponse.FromJson(res.ResponseStr);

				if (responseObj.Success)
				{
					if (responseObj.Data != null && responseObj.Data.ProductDiscounts != null)
					{
						var removedDiscounts = new List<Discount>();
						var remainingDiscounts = new List<Discount>();
						responseObj.Data.ProductDiscounts.ToList().ForEach(productDiscount => {
							var promotionDiscounts = productDiscount.Discounts.Where(d => d.Type == "Promotion");

							if (promotionDiscounts.Count() > 0)
							{
								var maxDiscount = promotionDiscounts.Aggregate((d1, d2) => d1.DiscountDiscount > d2.DiscountDiscount ? d1 : d2);

								var removingDiscounts = productDiscount.Discounts.Where(d => d.Type == "Promotion" && d != maxDiscount);
								if (removingDiscounts.Count() > 0)
								{
									removedDiscounts.AddRange(removingDiscounts);
								}

								var newDiscounts = productDiscount.Discounts.ToList();
								newDiscounts.RemoveAll(d => d.Type == "Promotion" && d != maxDiscount);
								remainingDiscounts.AddRange(newDiscounts);
								productDiscount.Discounts = newDiscounts.ToArray();

								productDiscount.Discount = productDiscount.Discounts.Sum(d => d.DiscountDiscount);
							}
						});

						if (removedDiscounts.Count > 0)
						{
							var notExistingDiscounts = removedDiscounts.Where(d => remainingDiscounts.Count(d1 => d1.Promotion != null && d1.Promotion.Name == d.Promotion.Name) == 0);
							responseObj.Data.ActivatedPromotions.RemoveAll(ap => notExistingDiscounts.Count(d => d.Promotion != null && d.Promotion.Name == ap.Name) > 0);
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
							cmd.CommandText = String.Format(@"SELECT ""Price"", ""Code"" FROM public.""ProductPrice"" WHERE ""Code"" IN({0})", String.Join(",", request.Products.Select(p => String.Format("'{0}'", p.ProductCode))));

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
						price = request.Products.Sum(p => Convert.ToDouble(p.Quantity) * prices.FirstOrDefault(pr => pr.Key == p.ProductCode).Value);

						var discounts = 0;

						if (responseObj.Data != null && responseObj.Data.ProductDiscounts != null)
						{
							discounts = Convert.ToInt32(responseObj.Data.ProductDiscounts.Sum(pd => pd.Discounts == null ? 0 : pd.Discounts.Sum(d => d.DiscountDiscount)));
						}

						var diff = Convert.ToInt32(price - Convert.ToDouble(request.Amount) + discounts);
						responseObj.BenefitAmount = diff.ToString();

						var now = DateTime.UtcNow;
						responseObj.BenefitFirst = AnswerTemplateCollection.CalculateResponseTemplatePrefix;
						responseObj.BenefitSecond = $"{GetDeclension(diff, "рубль", "рубля", "рублей")}. ";

						var rand = new Random();
						var prefixTemplate = AnswerTemplateCollection.Templates.OrderBy(t => rand.Next()).FirstOrDefault(t => t.IsFirstTextBlock && t.From <= diff && diff <= t.To && t.Start <= now && now <= t.End);
						if (prefixTemplate != null)
						{
							responseObj.BenefitSecond += $"{prefixTemplate.PrefixText} {Convert.ToInt32(prefixTemplate.Price != 0 ? diff / prefixTemplate.Price : 0)} {prefixTemplate.SuffixText} ";
						}

						var suffixTemplate = AnswerTemplateCollection.Templates.OrderBy(t => rand.Next()).FirstOrDefault(t => !t.IsFirstTextBlock && t.From <= diff && diff <= t.To && t.Start <= now && now <= t.End);
						if (suffixTemplate != null)
						{
							responseObj.BenefitSecond += $"{suffixTemplate.PrefixText} {Convert.ToInt32(suffixTemplate.Price != 0 ? diff / suffixTemplate.Price : 0)} {suffixTemplate.SuffixText}";
						}
					}

					responseObj.ActivePromocodes = Promocode.GetActivePromocodes(request.Client.MobilePhone, request.Client.CardNumber);
				}

				return Ok(responseObj);
			}
			else
			{
				return BadRequest(res.ResponseStr);
			}
		}

		[HttpPost("confirm")]
		public ActionResult Confirm([FromBody]PurchaseConfirmRequest request)
		{
			var authHeader = HttpContext.Request.Headers["Authorization"];
			if (authHeader.Count == 0) return Unauthorized();
			Logger.LogInfo("started processing request", "");
			var res = PRRequest("confirm", request.ToJson(), authHeader);
			Logger.LogInfo("done processing request", "");
			if (res.IsSuccess)
			{
				var responseObj = ConfirmResponse.FromJson(res.ResponseStr);
				if (request.Client != null && request.Client.MobilePhone == noNamePhone)
				{
					responseObj.Client = new ResponseClient() { Name = "NoName" };
				}
				else if (request.Client != null && request.Client.MobilePhone == sberbankPhone)
				{
					responseObj.Client = new ResponseClient() { Name = "Sberbank" };
				}

				if (responseObj.Success)
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
							cmd.CommandText = String.Format(@"SELECT ""Price"", ""Code"" FROM public.""ProductPrice"" WHERE ""Code"" IN({0})", String.Join(",", request.Products.Select(p => String.Format("'{0}'", p.ProductCode))));

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
						price = request.Products.Sum(p => Convert.ToDouble(p.Quantity) * prices.FirstOrDefault(pr => pr.Key == p.ProductCode).Value);

						var diff = Convert.ToInt32(price - Convert.ToDouble(request.Amount));
						responseObj.BenefitAmount = diff.ToString();

						var now = DateTime.UtcNow;
						responseObj.BenefitFirst = AnswerTemplateCollection.CalculateResponseTemplatePrefix;
						responseObj.BenefitSecond = $"{GetDeclension(diff, "рубль", "рубля", "рублей")}. ";

						var prefixTemplate = AnswerTemplateCollection.Templates.FirstOrDefault(t => t.IsFirstTextBlock && t.From <= diff && diff <= t.To && t.Start <= now && now <= t.End);
						if (prefixTemplate != null)
						{
							responseObj.BenefitSecond += $"{prefixTemplate.PrefixText} {Convert.ToInt32(prefixTemplate.Price != 0 ? diff / prefixTemplate.Price : 0)} {prefixTemplate.SuffixText} ";
						}

						var suffixTemplate = AnswerTemplateCollection.Templates.FirstOrDefault(t => !t.IsFirstTextBlock && t.From <= diff && diff <= t.To && t.Start <= now && now <= t.End);
						if (suffixTemplate != null)
						{
							responseObj.BenefitSecond += $"{suffixTemplate.PrefixText} {Convert.ToInt32(suffixTemplate.Price != 0 ? diff / suffixTemplate.Price : 0)} {suffixTemplate.SuffixText}";
						}
					}

					responseObj.ActivePromocodes = Promocode.GetActivePromocodes(request.Client.MobilePhone, request.Client.CardNumber);
				}
				Logger.LogInfo("return ok response", "");
				return Ok(responseObj);
			}
			else
			{
				return BadRequest(res.ResponseStr);
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

		private RequestResult PRRequest(string method, string body, StringValues authHeader)
		{
            var req = (HttpWebRequest)WebRequest.Create(string.Format("{0}/purchase/{1}", _processingUrl, method));
			req.Method = "POST";
			req.ContentType = "application/json";
			req.Accept = "application/json";
			req.Credentials = System.Net.CredentialCache.DefaultCredentials;
			req.Proxy.Credentials = System.Net.CredentialCache.DefaultCredentials;
			req.Timeout = 10 * 1000 * 60;
			req.Headers.Add("Authorization", authHeader);

			using (var requestStream = req.GetRequestStream())
			{
				using (var streamWriter = new StreamWriter(requestStream))
				{
					streamWriter.Write(body);
					streamWriter.Flush();
					streamWriter.Close();
				}
			}

			try
			{
				using (var response = req.GetResponse())
				{
					using (var responseStream = response.GetResponseStream())
					{
						using (var streamReader = new StreamReader(responseStream))
						{
							return new RequestResult() { IsSuccess = true, ResponseStr = streamReader.ReadToEnd() };
						}
					}
				}
			}
			catch (WebException e)
			{
				if (e.Response == null || ((HttpWebResponse)e.Response).StatusCode == HttpStatusCode.Unauthorized)
				{
					return new RequestResult() { IsSuccess = false, ResponseStr = e.Message };
				}

				using (var streamReader = new StreamReader(e.Response.GetResponseStream()))
				{
					var res = streamReader.ReadToEnd();
					return new RequestResult() { IsSuccess = true, ResponseStr = res };
				}
			}
		}
	}
}