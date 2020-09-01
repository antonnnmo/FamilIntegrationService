using FamilIntegrationCore.Models;
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
using Terrasoft.Core.Entities;

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

		public void StartShop()
		{
			Request("Main/Shop");
		}

		public void ExportContactsBalances()
		{
			while (ReadContactBalancePack())
			{
			}
		}

		private bool ReadContactBalancePack()
		{
			var pack = new List<ContactBalance>();
			var ids = new List<QueryColumnExpression>();
			var select = new Select(_uc)
						.Top(500)
						.Column("q", "Id")
						.Column("q", "SmrContactId")
						.Column("q", "SmrBalance")
						.Column("c", "SmrERPId")
						.From("SmrBalanceUpdateQueue").As("q")
						.LeftOuterJoin("Contact").As("c").On("c", "Id").IsEqual("q", "SmrContactId")
						.OrderByAsc("q", "CreatedOn") as Select;

			using (var dbExecutor = _uc.EnsureDBConnection())
			{
				using (var reader = select.ExecuteReader(dbExecutor))
				{
					while (reader.Read())
					{
						pack.Add(new ContactBalance()
						{
							Balance = reader.GetValue("SmrBalance", 0m),
							ERPId = reader.GetValue("SmrERPId", String.Empty)
						});

						ids.Add(Column.Parameter(reader.GetValue("Id", Guid.Empty)));
					}
				}
			}

			if (pack.Count == 0) return false;

			if (Request(_uri, "Main/ExportContactBalance", JsonConvert.SerializeObject(pack)).Item1)
			{
				new Delete(_uc).From("SmrBalanceUpdateQueue").Where("Id").In(ids).Execute();
			}

			return true;
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
						templates.Add(new AnswerTemplate()
						{
							End = reader.GetValue("SmrEndDate", DateTime.MaxValue),
							Start = reader.GetValue("SmrStartDate", DateTime.MinValue),
							From = reader.GetValue("SmrFrom", 0m),
							To = reader.GetValue("SmrTo", 0m),
							Price = reader.GetValue("SmrPrice", 0),
							IsFirstTextBlock = reader.GetValue("IsFirstTextBlock", 0) == 1,
							PrefixText = reader.GetValue("SmrText1", String.Empty),
							SuffixText = reader.GetValue("SmrText2", String.Empty)
						});
					}
				}
			}

			return Request(_processingMiddleWare, "Main/LoadAnswerTemplate", JsonConvert.SerializeObject(templates)).Item2;
		}

		private Tuple<bool, string> Request(string uri, string method, string body)
		{
			var req = (HttpWebRequest)WebRequest.Create(String.Format("{0}/api/{1}", uri, method));
			req.Method = "POST";
			req.Timeout = 10 * 1000 * 60;
			req.ContentType = "application/json";
			req.Accept = "application/json";

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
							return new Tuple<bool, string>(true, streamReader.ReadToEnd());
						}
					}
				}
			}
			catch (WebException e)
			{
				if (e.Response == null) throw new Exception(e.Message);
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

