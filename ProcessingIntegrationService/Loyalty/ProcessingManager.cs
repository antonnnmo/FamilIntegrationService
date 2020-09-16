using FamilIntegrationCore.Models;
using FamilIntegrationService;
using FamilIntegrationService.Providers;
using Microsoft.Extensions.Primitives;
using System.IO;
using System.Net;

namespace LoyaltyMiddleware.Loyalty
{
	public class ProcessingManager
	{
		protected string _uri;

		public ProcessingManager()
		{
			GlobalCacheReader.GetValue(GlobalCacheReader.CacheKeys.ProcessingUri, out _uri);
		}

		public RequestResult PRRequest(string method, string body, StringValues authHeader)
		{
			var req = (HttpWebRequest)WebRequest.Create(string.Format("{0}/{1}", _uri, method));
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
				if (e.Response == null)
				{
					Logger.LogError($"{method} error: ", e);
					return new RequestResult() { IsSuccess = false, ResponseStr = e.Message };
				}

				if(((HttpWebResponse)e.Response).StatusCode == HttpStatusCode.Unauthorized) return new RequestResult() { IsSuccess = false, IsUnathorized = true };

				using (var streamReader = new StreamReader(e.Response.GetResponseStream()))
				{
					var res = streamReader.ReadToEnd();
					Logger.LogError($"{method} error. {res}", e);
					return new RequestResult() { IsSuccess = false, ResponseStr = res };
				}
			}
		}
	}
}
