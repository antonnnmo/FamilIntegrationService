using FamilIntegrationCore.Models;
using FamilIntegrationService.Providers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FamilIntegrationService
{
	public class ContactTagManager : BaseManager
	{
		private static readonly string _selectQuery = @"SELECT TOP ({0}) [gateId]
      ,[ERPId]
      ,[source]
      ,[status]
      ,[errorMessage]
      ,CONVERT(nvarchar(50), createdOn, 21) as CreatedOn
      ,[name]
      ,[contactId]
  FROM [ContactTagGate]
 Where Status = 0 And Source = 0";

		public ContactTagManager()
		{
			_tableName = "ContactTagGate";
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
							pack.Add(new ContactTag()
							{
								Name = reader.GetValue("name", String.Empty),
								ERPId = reader.GetValue("ERPId", String.Empty),
								ContactId = reader.GetValue("contactId", String.Empty),
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
			return JsonConvert.SerializeObject(pack.Select(p => (ContactTag)p).ToList());
		}
	}
}
