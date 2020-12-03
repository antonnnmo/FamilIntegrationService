using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace FamilServiceMonitoringApp.Processing
{
	public class CalculateProcessor
	{
		public static ResponseResult Calculate()
		{
			return Request(@"{
									""client"": {
										""id"": ""e87da44a-091c-4237-8c54-bbe641c48588""
									},
									""date"": ""2020-12-01"",
									""shopCode"": ""370"",
									""cashDeskCode"": ""102"",
									""products"": [
										{
											""index"": 1,
											""price"": 100,
											""productCode"": ""89138765"",
											""quantity"": 1,
											""amount"": 100
										}
									],
									""paymentForm"": ""FullPayment"",
									""amount"": 100
								}");
		}

		private static ResponseResult Request(string body)
		{
			var req = (HttpWebRequest)WebRequest.Create("https://loyalty.famil.ru/purchase/calculate");
			req.Method = "POST";
			req.ContentType = "application/json";
			req.Accept = "application/json";
			req.Credentials = System.Net.CredentialCache.DefaultCredentials;
			req.Proxy.Credentials = System.Net.CredentialCache.DefaultCredentials;
			req.Headers.Add("Authorization", "Bearer vz1TaVi00aYl5nCCr0fJnBQU");

			using (var requestStream = req.GetRequestStream())
			{
				using (var streamWriter = new StreamWriter(requestStream))
				{
					streamWriter.Write(body);
					streamWriter.Flush();
					streamWriter.Close();
				}
			}

			System.Diagnostics.Stopwatch timer = new Stopwatch();

			timer.Start();

			try
			{
				using (var response = req.GetResponse())
				{
					timer.Stop();
					return new ResponseResult() { Time = timer.ElapsedMilliseconds, IsError = false };
				}
			}
			catch (WebException e)
			{
				timer.Stop();
				return new ResponseResult() { Time = timer.ElapsedMilliseconds, IsError = true };
			}
		}
	}
}
