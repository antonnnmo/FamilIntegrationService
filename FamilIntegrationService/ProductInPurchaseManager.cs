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
	public class ProductInPurchaseManager : BaseManager
	{
		private static readonly string _selectQuery = @"SELECT TOP ({0}) 
			  [gateId]
			  ,[ERPId]
			  ,[source]
			  ,[status]
			  ,[errorMessage]
			  ,CONVERT(nvarchar(50), createdOn, 21) as CreatedOn
			  ,[PurchaseId]
			  ,[ProductCode]
			  ,[Price]
			  ,[Quantity]
			  ,[amount]
		  FROM [ProductsInPurchaseGate]
		 Where Status = 0 And Source = 0";

		protected List<Product> _objects;

		public ProductInPurchaseManager()
		{
			_tableName = "ProductsInPurchaseGate";
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
							pack.Add(new ProductInPurchase()
							{
								Price = reader.GetValue("Price", 0m),
								ERPId = reader.GetValue("ERPId", String.Empty),
								ProductCode = reader.GetValue("ProductCode", String.Empty),
								Amount = reader.GetValue("Amount", 0m),
								PurchaseId = reader.GetValue("PurchaseId", String.Empty),
								Quantity = reader.GetValue("Quantity", 0),
								Id = Guid.NewGuid(),
							});
						}
					}
				}

				if (pack.Count > 0)
				{
					DBConnectionProvider.ExecuteNonQuery(String.Format("Update {1} Set Status = 3 Where ERPId in ({0})", String.Join(",", pack.Select(p => String.Format("'{0}'", p.ERPId))), _tableName));
				}
			}

			return pack;
		}

		protected override string GetSerializedCollection(List<BaseIntegrationObject> pack)
		{
			return JsonConvert.SerializeObject(pack.Select(p => (ProductInPurchase)p).ToList());
		}
	}
}

