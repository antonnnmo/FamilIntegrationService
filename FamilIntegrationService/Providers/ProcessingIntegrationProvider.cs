using FamilIntegrationCore.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace FamilIntegrationService.Providers
{
	public class ProcessingIntegrationProvider
	{
		//private string _login;
		//private string _password;
		private string _uri;
		//private CookieContainer _bpmCookieContainer;
		//private string _csrf;
		//private bool _useLocalCookie;

		public ProcessingIntegrationProvider()
		{
			GlobalCacheReader.GetValue(GlobalCacheReader.CacheKeys.ProcessingUri, out _uri);
		}

		public RequestResult Request(string method, string body)
		{
			var req = (HttpWebRequest)WebRequest.Create(String.Format("{0}/api/Main/{1}", _uri, method));
			req.Method = "POST";
			req.ContentType = "application/json";
			req.Accept = "application/json";
			req.Credentials = System.Net.CredentialCache.DefaultCredentials;
			req.Proxy.Credentials = System.Net.CredentialCache.DefaultCredentials;
			req.Timeout = 10 * 1000 * 60;

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
				using (var streamReader = new StreamReader(e.Response.GetResponseStream()))
				{
					var res = streamReader.ReadToEnd();
					return new RequestResult() { IsSuccess = false, ResponseStr = e.Message + " " + res };
				}
			}
		}
	}
}
