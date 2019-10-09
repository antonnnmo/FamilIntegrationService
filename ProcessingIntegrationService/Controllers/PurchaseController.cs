using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FamilIntegrationCore.Models;
using FamilIntegrationService;
using FamilIntegrationService.Providers;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

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

		[HttpPost("confirm")]
		public ActionResult Confirm([FromBody]PurchaseConfirmRequest request)
		{
			var res = PRRequest(request.ToJson());

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
					var productCodes = new CalcProductRetailPriceRequest() { ProductCodes = request.Products.Select(p => p.ProductCode).ToList() };
					var result = new CRMIntegrationProvider().MakeRequest("MiddlewareProcessingService/CalcProductRetailPrice", String.Format(@"{{""request"": {0}}}", JsonConvert.SerializeObject(productCodes)));
					if (result.IsSuccess)
					{
						var price = ((CalcProductRetailPriceResponse)JsonConvert.DeserializeObject(result.ResponseStr, typeof(CalcProductRetailPriceResponse))).CalcProductRetailPriceResult;
						var diff = Convert.ToInt32(price - request.Amount);
						responseObj.BenefitAmount = $"{diff} {GetDeclension(diff, "рубль", "рубля", "рублей")}";

						var now = DateTime.UtcNow;
						responseObj.BenefitSecond = String.Empty;

						var prefixTemplate = AnswerTemplateCollection.Templates.FirstOrDefault(t => t.IsFirstTextBlock && t.From <= diff && diff <= t.To && t.Start <= now && now <= t.End);
						if (prefixTemplate != null)
						{
							responseObj.BenefitSecond += $"{prefixTemplate.PrefixText} {Convert.ToInt32(prefixTemplate.Price != 0 ? diff /prefixTemplate.Price : 0)} {prefixTemplate.SuffixText} ";
						}

						var suffixTemplate = AnswerTemplateCollection.Templates.FirstOrDefault(t => !t.IsFirstTextBlock && t.From <= diff && diff <= t.To && t.Start <= now && now <= t.End);
						if (suffixTemplate != null)
						{
							responseObj.BenefitSecond += $"{suffixTemplate.PrefixText} {Convert.ToInt32(suffixTemplate.Price != 0 ? diff / suffixTemplate.Price : 0)} {suffixTemplate.SuffixText}";
						}
					}
					return Ok(responseObj);
				}
				else
				{
					return BadRequest(responseObj);
				}
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

		private RequestResult PRRequest(string body)
		{
            var req = (HttpWebRequest)WebRequest.Create(string.Format("{0}/purchase/confirm", _processingUrl));
			req.Method = "POST";
			req.ContentType = "application/json";
			req.Accept = "application/json";
			req.Credentials = System.Net.CredentialCache.DefaultCredentials;
			req.Proxy.Credentials = System.Net.CredentialCache.DefaultCredentials;
			req.Timeout = 10 * 1000 * 60;
			req.Headers.Add("Authorization", string.Format("Bearer {0}", _processingSecret));

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
				if (e.Response == null)
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