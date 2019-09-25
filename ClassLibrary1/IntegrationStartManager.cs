using FamilIntegrationService;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Terrasoft.Core;
using Terrasoft.Core.DB;

namespace Terrasoft.Configuration
{
	public class IntegrationStartManager
	{
		private string _uri;
		private string _processingMiddleWare;
		private UserConnection _uc;

		public IntegrationStartManager(UserConnection uc)
		{
			_uc = uc;
			_uri = Terrasoft.Core.Configuration.SysSettings.GetValue(uc, "GateIntegrationService", String.Empty);
			_processingMiddleWare = Terrasoft.Core.Configuration.SysSettings.GetValue(uc, "ProcessingMiddlewareUri", String.Empty);
		}

		public void StartContact(bool isPrimary)
		{
			if (isPrimary)
			{
				Request("Main/Primary");
			}
			else
			{
				Request("Main");
			}
		}

		public void StartProductSize(bool isPrimary)
		{
			if (isPrimary)
			{
				Request("Main/PrimaryProductSize");
			}
			else
			{
				Request("Main/ProductSize");
			}
		}

		public void StartProductTag(bool isPrimary)
		{
			if (isPrimary)
			{
				Request("Main/PrimaryProductTag");
			}
			else
			{
				Request("Main/ProductTag");
			}
		}

		public void StartContactTag(bool isPrimary)
		{
			if (isPrimary)
			{
				Request("Main/PrimaryContactTag");
			}
			else
			{
				Request("Main/ContactTag");
			}
		}

		public void StartProductCategory(bool isPrimary)
		{
			if (isPrimary)
			{
				Request("Main/PrimaryProductCategory");
			}
			else
			{
				Request("Main/ProductCategory");
			}
		}

		public void StartProductSubCategory(bool isPrimary)
		{
			if (isPrimary)
			{
				Request("Main/PrimaryProductSubCategory");
			}
			else
			{
				Request("Main/ProductSubCategory");
			}
		}

		public void StartBrandType(bool isPrimary)
		{
			if (isPrimary)
			{
				Request("Main/PrimaryBrandType");
			}
			else
			{
				Request("Main/BrandType");
			}
		}

		public void StartProductGroup(bool isPrimary)
		{
			if (isPrimary)
			{
				Request("Main/PrimaryProductGroup");
			}
			else
			{
				Request("Main/ProductGroup");
			}
		}

		public void StartSMS(bool isPrimary)
		{
			if (isPrimary)
			{
				Request("Main/PrimarySMS");
			}
			else
			{
				Request("Main/SMS");
			}
		}

		public void StartPurchase(bool isPrimary)
		{
			if (isPrimary)
			{
				Request("Main/PrimaryPurchase");
			}
			else
			{
				Request("Main/Purchase");
			}
		}

		public void StartPurchaseProduct()
		{
			Request("Main/PrimaryProductInPurchase");
		}

		public void StartPaymentInPurchase()
		{
			Request("Main/PrimaryPaymentInPurchase");
		}

		public void StartBrand()
		{
			Request("Main/Brand");
		}

		public string UpdateProcessingAnswerTemplate()
		{
			var templates = new List<AnswerTemplate>();
			var select = new Select(_uc)
				.Column("SmrText1")
				.Column("SmrText2")
				.Column("SmrFrom")
				.Column("SmrTo")
				.Column("SmrPrice")
				.Column("SmrStartDate")
				.Column("SmrEndDate")
				.Column(Column.SqlText("1")).As("IsFirstTextBlock")
				.From("SmrCalculateAnswerTemplate")
			.Union(new Select(_uc)
				.Column("SmrText1")
				.Column("SmrText2")
				.Column("SmrFrom")
				.Column("SmrTo")
				.Column("SmrPrice")
				.Column("SmrStartDate")
				.Column("SmrEndDate")
				.Column(Column.SqlText("0")).As("IsFirstTextBlock")
				.From("SmrCalculateAnswerTemplateSecond")) as Select;

			using (var dbExecutor = _uc.EnsureDBConnection())
			{
				using (var reader = select.ExecuteReader(dbExecutor))
				{
					while (reader.Read())
					{
						templates.Add(new AnswerTemplate() {
							End = reader.GetValue("SmrEndDate", DateTime.MaxValue),
							Start = reader.GetValue("SmrStartDate", DateTime.MinValue),
							From = reader.GetValue("SmrFrom", 0m),
							To = reader.GetValue("SmrTo", 0m),
							Price = reader.GetValue("SmrPrice", 0),
							IsFirstTextBlock = reader.GetValue("IsFirstTextBlock", false),
							PrefixText = reader.GetValue("SmrText1", String.Empty),
							SuffixText = reader.GetValue("SmrText2", String.Empty)
						});
					}
				}
			}

			return Request(_processingMiddleWare, "Main/LoadAnswerTemplate", JsonConvert.SerializeObject(templates));
		}

		private string Request(string uri, string method, string body)
		{
			var req = (HttpWebRequest)WebRequest.Create(String.Format("{0}/api/{1}", uri, method));
			req.Method = "POST";
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
							return streamReader.ReadToEnd();
						}
					}
				}
			}
			catch (WebException e)
			{
				using (var streamReader = new StreamReader(e.Response.GetResponseStream()))
				{
					var res = streamReader.ReadToEnd();
					throw new Exception(e.Message + " " + res);
				}
			}
		}

		private string Request(string method)
		{
			var req = (HttpWebRequest)WebRequest.Create(String.Format("{0}/api/{1}", _uri, method));
			req.Method = "GET";
			req.Timeout = 10 * 1000 * 60;

			try
			{
				using (var response = req.GetResponse())
				{
					using (var responseStream = response.GetResponseStream())
					{
						using (var streamReader = new StreamReader(responseStream))
						{
							return streamReader.ReadToEnd();
						}
					}
				}
			}
			catch (WebException e)
			{
				using (var streamReader = new StreamReader(e.Response.GetResponseStream()))
				{
					var res = streamReader.ReadToEnd();
					throw new Exception(e.Message + " " + res);
				}
			}
		}
	}
}

