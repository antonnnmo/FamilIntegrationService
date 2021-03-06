﻿using FamilIntegrationCore.Models;
using System;
using System.IO;
using System.Net;

namespace FamilIntegrationService.Providers
{
	public class CRMIntegrationProvider
	{
		private string _login;
		private string _password;
		private string _uri;
		private CookieContainer _bpmCookieContainer;
		private string _csrf;
		private bool _useLocalCookie;
        private int _timeout;

        public CRMIntegrationProvider(bool useLocalCookie = false)
		{
			_useLocalCookie = useLocalCookie;
			GlobalCacheReader.GetValue(GlobalCacheReader.CacheKeys.BPMLogin, out _login);
			GlobalCacheReader.GetValue(GlobalCacheReader.CacheKeys.BPMPassword, out _password);
			GlobalCacheReader.GetValue(GlobalCacheReader.CacheKeys.BPMUri, out _uri);
            GlobalCacheReader.GetValue<int>(GlobalCacheReader.CacheKeys.CrmRequestTimeout, out _timeout);
			Authorize();
        }

		//public void SendSaleToCRM(Guid saleId, bool isNeedCreateProject = false)
		//{
		//	var context = new JiraMonitorContext();
		//	var sale = context.Sale.FirstOrDefault(s => s.Id == saleId);
		//	if (sale != null)
		//	{
		//		MakeRequest("PTTrackerIntegrationService/UpdateOpportunity", String.Format(@"{{""oppId"": ""{0}"", ""statusId"": ""{1}"", ""resultId"":""{2}"", ""isNeedCreateProject"":""{3}""}}", sale.CrmId, sale.StatusId, sale.ResultId ?? Guid.Empty, isNeedCreateProject ? "1" : "0"));
		//	}
		//}

		public RequestResult MakeRequest(string bpmServiceUri, string body)
		{
			return Request(bpmServiceUri, body);
		}

		private void CheckAndAuth(out CookieContainer bpmCookieContainer)
		{
			HttpWebRequest req;
			bpmCookieContainer = null;
			if (_useLocalCookie)
			{
				if (_bpmCookieContainer == null)
				{
					Authorize();
				}
			}
			else
			{
				if (!GlobalCacheReader.GetValue(GlobalCacheReader.CacheKeys.BPMCookie, out bpmCookieContainer))
				{
					Authorize();
				}
			}
		}

		private string Authorize()
		{
            //Вызов сервиса с авторизацией    
            var req = (HttpWebRequest)WebRequest.Create(String.Format("{0}/ServiceModel/AuthService.svc/Login", _uri));

			req.Method = "POST";
			req.ContentType = "application/json";
			req.Accept = "application/json";
			var bpmCookieContainer = new CookieContainer();
			req.CookieContainer = bpmCookieContainer;

			req.Credentials =
				   System.Net.CredentialCache.DefaultCredentials;

			req.Proxy.Credentials =
				   System.Net.CredentialCache.DefaultCredentials;

			using (var streamWriter = new StreamWriter(req.GetRequestStream()))
			{
				string json = "{\"UserName\":\"" + _login + "\",\"UserPassword\":\"" + _password + "\"}";

				streamWriter.Write(json);
				streamWriter.Flush();
				streamWriter.Close();
			}

			var httpResponse = req.GetResponse();
			var cookies = bpmCookieContainer.GetCookies(new Uri(_uri));
			try
			{
				if (_useLocalCookie)
				{
					_bpmCookieContainer = bpmCookieContainer;
					_csrf = cookies["BPMCSRF"].Value;
				}
				else
				{
					GlobalCacheReader.SetTemporaryValue(GlobalCacheReader.CacheKeys.BPMCookie, bpmCookieContainer, TimeSpan.FromMinutes(20));
					GlobalCacheReader.SetTemporaryValue(GlobalCacheReader.CacheKeys.BPMCSRF, cookies["BPMCSRF"].Value, TimeSpan.FromMinutes(20));
				}

				return cookies["BPMCSRF"].Value;
			}
			catch
			{
				return String.Empty;
			}
		}

		private RequestResult Request(string bpmServiceUri, string body, bool isRepeatOnNonAuth = true)
		{
			CookieContainer bpmCookieContainer;
			if (_useLocalCookie)
			{
				bpmCookieContainer = _bpmCookieContainer;
			}
			else
			{
				GlobalCacheReader.GetValue(GlobalCacheReader.CacheKeys.BPMCookie, out bpmCookieContainer);
			}

			var req = (HttpWebRequest)WebRequest.Create(String.Format("{0}/0/rest/{1}", _uri, bpmServiceUri));
			req.Method = "POST";
			req.ContentType = "application/json";
			req.Accept = "application/json";
			req.CookieContainer = bpmCookieContainer;
			req.Credentials = System.Net.CredentialCache.DefaultCredentials;
			req.Proxy.Credentials = System.Net.CredentialCache.DefaultCredentials;
			req.Timeout = _timeout;

			var csrfToken = String.Empty;

			if (_useLocalCookie) csrfToken = _csrf;
			else GlobalCacheReader.GetValue(GlobalCacheReader.CacheKeys.BPMCSRF, out csrfToken);

            if (!String.IsNullOrEmpty(csrfToken))
			{
				req.Headers.Add("BPMCSRF", csrfToken);
			}

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
				if (e.Status == WebExceptionStatus.Timeout)
				{
					Logger.LogError($"{bpmServiceUri}", e);
					return new RequestResult() { IsSuccess = false, ResponseStr = e.Message, IsTimeout = true };
				}
				if (e.Response == null)
				{
					Logger.LogError($"{bpmServiceUri}", e);
					return new RequestResult() { IsSuccess = false, ResponseStr = e.Message + " null response" };
				}
				else if (((HttpWebResponse)e.Response).StatusCode == HttpStatusCode.Unauthorized && isRepeatOnNonAuth)
				{
					Authorize();
					return Request(bpmServiceUri, body, false);
				}
				using (var streamReader = new StreamReader(e.Response.GetResponseStream()))
				{
					var res = streamReader.ReadToEnd();
					Logger.LogError($"{bpmServiceUri} {res}", e);
					return new RequestResult() { IsSuccess = false, ResponseStr = e.Message + " " + res };
				}
			}
		}
	}
}
