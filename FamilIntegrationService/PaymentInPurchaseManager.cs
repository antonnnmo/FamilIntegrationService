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
	public class PaymentInPurchaseManager : BaseManager
	{
		private static readonly string _selectQuery = @"SELECT TOP ({0}) 
			  [gateId]
			  ,[ERPId]
			  ,[source]
			  ,[status]
			  ,[errorMessage]
			  ,CONVERT(nvarchar(50), createdOn, 21) as CreatedOn
			  ,[PurchaseId]
			  ,[Type]
			  ,[Amount]
		  FROM [PaymentsInPurchaseGate]
		 Where Status = 0 And Source = 0";

		protected List<Product> _objects;

		public PaymentInPurchaseManager()
		{
			_tableName = "PaymentsInPurchaseGate";
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
							pack.Add(new PaymentInPurchase()
							{
								ERPId = reader.GetValue("ERPId", String.Empty),
								Amount = reader.GetValue("Amount", 0m),
								PurchaseId = reader.GetValue("PurchaseId", String.Empty),
								Type = reader.GetValue("Type", String.Empty),
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

		protected override string GetSerializedCollection(List<BaseIntegrationObject> pack)
		{
			return JsonConvert.SerializeObject(pack.Select(p => (PaymentInPurchase)p).ToList());
		}
	}
}

