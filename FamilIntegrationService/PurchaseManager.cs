using FamilIntegrationCore.Models;
using FamilIntegrationService.Models;
using FamilIntegrationService.Providers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace FamilIntegrationService
{
	public class PurchaseManager : BaseManager
	{
		private static readonly string _selectQuery = @"SELECT TOP ({0}) 
			  [gateId]
			  ,[ERPId]
			  ,[source]
			  ,[status]
			  ,[errorMessage]
			  ,CONVERT(nvarchar(50), createdOn, 21) as CreatedOn
			  ,[number]
			  ,[contactId]
			  ,[phone]
			  ,[cardNumber]
			  ,CONVERT(nvarchar(50), purchaseDate, 21) as purchaseDateStr
			  ,purchaseDate
			  ,[shopCode]
			  ,[cashDeskCode]
			  ,[paymentForm]
			  ,[amount]
		  FROM [PurchaseGate]
		 Where Status = 0 And Source = 0";

		protected List<Product> _objects;

		public PurchaseManager()
		{
			_tableName = "PurchaseGate";
			_isNeedSendToProcessing = false;
		}

		protected override List<BaseIntegrationObject> ReadPack()
		{
			var pack = new List<BaseIntegrationObject>();
			lock (_lock)
			{
				using (var provider = new DBConnectionProvider())
				{
					using (var reader = provider.Execute(_selectQuery, packSize))
					{
						while (reader != null && reader.Read())
						{
							pack.Add(new Purchase()
							{
								Number = reader.GetValue("Number", String.Empty),
								ERPId = reader.GetValue("ERPId", String.Empty),
								CardNumber = reader.GetValue("CardNumber", String.Empty),
								Amount = reader.GetValue("Amount", 0m),
								CashDeskCode = reader.GetValue("CashDeskCode", String.Empty),
								ContactId = reader.GetValue("ContactId", String.Empty),
								CreatedOn = reader.GetValue("CreatedOn", String.Empty),
								PaymentForm = reader.GetValue("PaymentForm", String.Empty),
								Phone = reader.GetValue("Phone", String.Empty),
								PurchaseDate = reader.GetValue("purchaseDateStr", String.Empty),
								PDate = reader.GetValue("purchaseDate", DateTime.MinValue),
								ShopCode = reader.GetValue("ShopCode", String.Empty),
								Id = Guid.NewGuid(),
							});
						}
					}
				}

				if (pack.Count > 0)
				{
					DBConnectionProvider.ExecuteNonQuery(String.Format("Update {1} Set Status = 3 Where ERPId in ({0})", String.Join(",", pack.Select(p => String.Format("'{0}'", p.CorrectERPId))), _tableName));
				}
			}

			return pack;
		}

		public override void Execute()
		{
			Logger.LogInfo("Начался импорт", _tableName);

			var tasks = new List<Task>();
			for (var j = 0; j < threadCount; j++)
			{
				var task = new Task(() =>
				{
					var pack = ReadPack();
					while (pack.Count > 0)
					{
						var results = new PackResults() {  IntegratePackResult = new List<PackResult>()};
						foreach (Purchase purchase in pack)
						{
							purchase.ProductsInPurchase = ReadProducts(purchase.ERPId);
							purchase.PaymentsInPurchase = ReadPayments(purchase.ERPId);

							int i = 0;
							var request = new PurchaseConfirmRequest()
							{
								//CashdeskCode = purchase.CashDeskCode,
								Client = new Client()
								{
									CardNumber = purchase.CardNumber,
									MobilePhone = purchase.Phone
								},
								Date = purchase.PDate,
								Number = purchase.Number,
								PaymentForm = GetPaymentFormCode(purchase.PaymentForm),
								ShopCode = purchase.ShopCode,
								Payments = purchase.PaymentsInPurchase.Select(p => new Payment() { Amount = p.Amount, Type = p.Type }),
								Products = purchase.ProductsInPurchase.Select(p => new PCProduct() { Index = i++, Amount = p.Amount, Price = p.Price, ProductCode = p.ProductCode, Quantity = p.Quantity }),
								Id = Guid.NewGuid().ToString()
							};

                            var res = Request(request.ToJson());
							Logger.LogInfo(i.ToString(), res.IsSuccess ? "success" : res.ResponseStr);

							if (res.IsSuccess)
							{
                                var resRequest = JsonConvert.DeserializeObject<PackResult>(res.ResponseStr);
                                resRequest.Id = purchase.ERPId;
                                results.IntegratePackResult.Add(resRequest);
							}
							else
							{
								results.IntegratePackResult.Add(new PackResult() { Id = purchase.ERPId, IsSuccess = false, ErrorMessage = res.ResponseStr });
							}
						}

						ProceedResults(results);
						pack = ReadPack();
					}
				});

				task.Start();
				tasks.Add(task);
			}

			Task.WaitAll(tasks.ToArray());

			Logger.LogInfo("Finished", _tableName);
		}

        private string GetPaymentFormCode(string name)
        {
            switch(name)
            {
                case "PurchaseGate": return "0";
                case "Credit": return "1";
                case "Installment plan": return "2";
            }
            return "0";
        }

		private List<ProductInPurchase> ReadProducts(string erpId)
		{
			var res = new List<ProductInPurchase>();

			using (var provider = new DBConnectionProvider())
			{
				using (var reader = provider.Execute(@"Select ProductCode, Price, Quantity, Amount from ProductsInPurchaseGate WITH(NOLOCK) Where PurchaseId = '{0}'", erpId))
				{
					while (reader != null && reader.Read())
					{
						res.Add(new ProductInPurchase() {
							Amount = reader.GetValue("Amount", 0m),
							Price = reader.GetValue("Price", 0m),
							Quantity = reader.GetValue("Quantity", 0),
							ProductCode = reader.GetValue("ProductCode", String.Empty),
						});
					}
				}
			}

			return res;
		}

		private List<PaymentInPurchase> ReadPayments(string erpId)
		{
			var res = new List<PaymentInPurchase>();

			using (var provider = new DBConnectionProvider())
			{
				using (var reader = provider.Execute(@"Select Type, Amount from PaymentsInPurchaseGate WITH(NOLOCK) Where PurchaseId = '{0}'", erpId))
				{
					while (reader != null && reader.Read())
					{
						res.Add(new PaymentInPurchase()
						{
							Amount = reader.GetValue("Amount", 0m),
							Type = reader.GetValue("Type", String.Empty),
						});
					}
				}
			}

			return res;
		}

		public RequestResult Request(string body)
		{
            /*var req = (HttpWebRequest)WebRequest.Create("http://stnd-prsrv-07:5000/purchase/confirm");
			req.Method = "POST";
			req.ContentType = "application/json";
			req.Accept = "application/json";
			req.Credentials = System.Net.CredentialCache.DefaultCredentials;
			req.Proxy.Credentials = System.Net.CredentialCache.DefaultCredentials;
			req.Timeout = 10 * 1000 * 60;
			req.Headers.Add("Authorization", "Bearer secret");

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
			}*/
            var processingIntegrationProvider = new ProcessingIntegrationProvider();
            return processingIntegrationProvider.RequestMethod("purchase/confirm", body);
        }

		protected override string GetSerializedCollection(List<BaseIntegrationObject> pack)
		{
			return JsonConvert.SerializeObject(pack.Select(p => (Purchase)p).ToList());
		}
	}
}

