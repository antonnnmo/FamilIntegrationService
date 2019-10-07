using FamilIntegrationCore.Models;
using FamilIntegrationService.Providers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FamilIntegrationService
{
	public class ProductManager : BaseManager
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
			  ,[isArchived]
			  ,[direction]
			  ,[category]
			  ,[subCategory]
			  ,[group]
			  ,[brand]
			  ,[brandType]
			  ,[recommendedRetailPrice]
			  ,[provider]
			  ,[size]
		  FROM [ProductGate]
		 Where Status = 0 And Source = 0";

		protected List<Product> _objects;

		public ProductManager()
		{
			_tableName = "ProductGate";
			_isNeedSendToProcessing = true;
			_processingMethodName = "LoadProductPack";
			_processingPrimaryMethodName= "LoadPrimaryProductPack";
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
							pack.Add(new Product()
							{
								Name = reader.GetValue("name", String.Empty),
								ERPId = reader.GetValue("ERPId", String.Empty),
								Category = reader.GetValue("category", String.Empty),
								Brand = reader.GetValue("Brand", String.Empty),
								BrandType = reader.GetValue("BrandType", String.Empty),
								Code = reader.GetValue("Code", String.Empty),
								Direction = reader.GetValue("Direction", String.Empty),
								Group = reader.GetValue("Group", String.Empty),
								Provider = reader.GetValue("Provider", String.Empty),
								Size = reader.GetValue("Size", String.Empty),
								SubCategory = reader.GetValue("SubCategory", String.Empty),
								IsArchived = reader.GetValue("IsArchived", false),
								RecommendedRetailPrice = reader.GetValue("RecommendedRetailPrice", 0m),
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
			return JsonConvert.SerializeObject(pack.Select(p => (Product)p).ToList());
		}

		protected override string GetProcessingPackBody(List<BaseIntegrationObject> pack)
		{
			var contacts = pack.Select(c => (Product)c).Select(c => new ProductProcessingModel() { ERPId = c.ERPId, Id = GetCustomFieldsGuidValue(c, "ObjectId") ?? c.Id, Code = c.Code, Name = c.Name });
			return JsonConvert.SerializeObject(contacts);
		}
	}
}

