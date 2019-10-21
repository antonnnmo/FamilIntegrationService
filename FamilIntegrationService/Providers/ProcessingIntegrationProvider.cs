using FamilIntegrationCore.Models;
using Newtonsoft.Json;
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
		private string _login;
		private string _password;
		private string _uri;
		private string _token;
		private string _tokenCacheName;
		//private CookieContainer _bpmCookieContainer;
		//private string _csrf;
		//private bool _useLocalCookie;

		public ProcessingIntegrationProvider(bool isPA)
		{
			if (isPA)
			{
				GlobalCacheReader.GetValue(GlobalCacheReader.CacheKeys.PersonalAreaUri, out _uri);
				GlobalCacheReader.GetValue(GlobalCacheReader.CacheKeys.PersonalAreaLogin, out _login);
				GlobalCacheReader.GetValue(GlobalCacheReader.CacheKeys.PersonalAreaPasword, out _password);
				_tokenCacheName = GlobalCacheReader.CacheKeys.PersonalAreaToken;
				GlobalCacheReader.GetValue(_tokenCacheName, out _token);
			}
			else
			{
				GlobalCacheReader.GetValue(GlobalCacheReader.CacheKeys.ProcessingUri, out _uri);
				GlobalCacheReader.GetValue(GlobalCacheReader.CacheKeys.ProcessingLogin, out _login);
				GlobalCacheReader.GetValue(GlobalCacheReader.CacheKeys.ProcessingPasword, out _password);
				_tokenCacheName = GlobalCacheReader.CacheKeys.ProcessingToken;
				GlobalCacheReader.GetValue(_tokenCacheName, out _token);
			}

			if (String.IsNullOrEmpty(_token))
			{
				Authorize();
			}
		}

		public bool Authorize()
		{
			var req = (HttpWebRequest)WebRequest.Create(String.Format("{0}/api/Identity/Token", _uri));
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
					var body = JsonConvert.SerializeObject(new { Login = _login, Password = _password });
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
							_token = streamReader.ReadToEnd().Replace("\"", "");
							GlobalCacheReader.SetValue(_tokenCacheName, _token);
							return true;
						}
					}
				}
			}
			catch (WebException e)
			{
				Logger.LogError("Ошибка аутентификации в сервисе процессинга", e);
				return false;
			}
		}

		public RequestResult Request(string method, string body, bool isNeedRepeat = true)
		{
			var req = (HttpWebRequest)WebRequest.Create(String.Format("{0}/api/Main/{1}", _uri, method));
			req.Method = "POST";
			req.ContentType = "application/json";
			req.Accept = "application/json";
			req.Credentials = System.Net.CredentialCache.DefaultCredentials;
			req.Proxy.Credentials = System.Net.CredentialCache.DefaultCredentials;
			req.Timeout = 10 * 1000 * 60;
			req.Headers.Add("Authorization", $"Bearer {_token}");

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

				if (((HttpWebResponse)e.Response).StatusCode == HttpStatusCode.Unauthorized)
				{
					if (Authorize())
					{
						if (isNeedRepeat) return Request(method, body, false);
					}
				}

				using (var streamReader = new StreamReader(e.Response.GetResponseStream()))
				{
					var res = streamReader.ReadToEnd();
					return new RequestResult() { IsSuccess = false, ResponseStr = e.Message + " " + res };
				}
			}
			catch (Exception e) {
					return new RequestResult() { IsSuccess = false, ResponseStr = e.Message };
			}
		}

        public RequestResult RequestMethod(string method, string body, bool isNeedRepeat = true)
        {
            var req = (HttpWebRequest)WebRequest.Create(String.Format("{0}/{1}", _uri, method));
            req.Method = "POST";
            req.ContentType = "application/json";
            req.Accept = "application/json";
            req.Credentials = System.Net.CredentialCache.DefaultCredentials;
            req.Proxy.Credentials = System.Net.CredentialCache.DefaultCredentials;
            req.Timeout = 10 * 1000 * 60;
            req.Headers.Add("Authorization", $"Bearer {_token}");

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

                if (((HttpWebResponse)e.Response).StatusCode == HttpStatusCode.Unauthorized)
                {
                    if (Authorize())
                    {
                        if (isNeedRepeat) return Request(method, body, false);
                    }
                }

                using (var streamReader = new StreamReader(e.Response.GetResponseStream()))
                {
                    var res = streamReader.ReadToEnd();
                    return new RequestResult() { IsSuccess = false, ResponseStr = e.Message + " " + res };
                }
            }
            catch (Exception e)
            {
                return new RequestResult() { IsSuccess = false, ResponseStr = e.Message };
            }
        }
    }
}
