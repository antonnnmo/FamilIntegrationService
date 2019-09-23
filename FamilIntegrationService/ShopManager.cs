using FamilIntegrationCore.Models;
using FamilIntegrationService.Models;
using FamilIntegrationService.Providers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FamilIntegrationService
{
	public class ShopManager : BaseManager
	{
		private static readonly string _selectQuery = @"SELECT TOP ({0}) 
			  [gateId]
			  ,[ERPId]
			  ,[source]
			  ,[status]
			  ,[errorMessage]
			  ,CONVERT(nvarchar(50), createdOn, 21) as CreatedOn
			  ,[name]
			  ,[code]
			  ,[cluster]
			  ,[city]
			  ,[description]
		  FROM [ShopGate]
		 Where Status = 0 And Source = 0";

		protected List<BrandType> _objects;

		public ShopManager()
		{
			_tableName = "ShopGate";
			_isNeedSendToProcessing = true;
			_processingPrimaryMethodName = "LoadPrimaryShopPack";
			_processingMethodName = "LoadShopPack";
		}

		protected override string GetProcessingPackBody(List<BaseIntegrationObject> pack)
		{
			var contacts = pack.Select(c => (Shop)c).Select(c => new ShopProcessingModel() { ERPId = c.ERPId, Id = c.Id, Name = c.Name, Code = c.Code});
			return JsonConvert.SerializeObject(contacts);
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
							pack.Add(new Shop()
							{
								Name = reader.GetValue("name", String.Empty),
								ERPId = reader.GetValue("ERPId", String.Empty),
								City = reader.GetValue("City", String.Empty),
								Cluster = reader.GetValue("Cluster", String.Empty),
								Code = reader.GetValue("Code", String.Empty),
								Description = reader.GetValue("Description", String.Empty),
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
			return JsonConvert.SerializeObject(pack.Select(p => (Shop)p).ToList());
		}
	}
}

