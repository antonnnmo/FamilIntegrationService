using FamilIntegrationCore.Models;
using FamilIntegrationService.Providers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FamilIntegrationService
{
	public class BrandTypeManager : BaseManager
	{
		private static readonly string _selectQuery = @"SELECT TOP ({0}) 
			  [gateId]
			  ,[ERPId]
			  ,[source]
			  ,[status]
			  ,[errorMessage]
			  ,CONVERT(nvarchar(50), createdOn, 21) as CreatedOn
			  ,[name]
		  FROM [BrandTypeGate]
		 Where Status = 0 And Source = 0";

		protected List<BrandType> _objects;

		public BrandTypeManager()
		{
			_tableName = "BrandTypeGate";
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
							pack.Add(new BrandType()
							{
								Name = reader.GetValue("name", String.Empty),
								ERPId = reader.GetValue("ERPId", String.Empty),
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
			return JsonConvert.SerializeObject(pack.Select(p => (BrandType)p).ToList());
		}
	}
}

