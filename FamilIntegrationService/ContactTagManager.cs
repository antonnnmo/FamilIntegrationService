using FamilIntegrationCore.Models;
using FamilIntegrationService.Models;
using FamilIntegrationService.Providers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            threadCount = 1;
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
								Id = reader.GetValue("GateId", Guid.Empty),
							});
						}
					}
				}

				if (pack.Count > 0)
				{
					DBConnectionProvider.ExecuteNonQuery(String.Format("Update {1} Set Status = 3 Where gateId in ({0})", String.Join(",", pack.Select(p => String.Format("'{0}'", p.Id))), _tableName));
				}
			}

			return pack;
		}

        public override void ProceedResult(PackResult result, List<BaseIntegrationObject> pack)
        {
            lock (_lockRes)
            {
                try
                {
                    var query = new StringBuilder();

                    if (result.IsSuccess)
                    {
                        foreach (var obj in pack)
                        {
                            query.AppendLine(String.Format("Update {1} Set Status = 1 Where gateId = '{0}';", obj.Id, _tableName));
                        }
                    }
                    else
                    {
                        var errorMessage = String.IsNullOrEmpty(result.ErrorMessage) ? String.Empty : result.ErrorMessage.Replace("'", "").Replace("{", "").Replace("}", "");
                        if (errorMessage.Length > 250) errorMessage = errorMessage.Substring(0, 250);
                        foreach (var obj in pack)
                        {
                            query.AppendLine(String.Format("Update {2} Set Status = 2, ErrorMessage = '{1}' Where gateId = '{0}';", obj.Id, errorMessage, _tableName));
                        }
                    }

                    var sql = query.ToString();
                    if (!string.IsNullOrEmpty(sql))
                        DBConnectionProvider.ExecuteNonQuery(sql);
                }
                catch (Exception e)
                {
                    Logger.LogError(String.Format("Ошибка обновления состояний в ШТ {0} для первичного импорта", _tableName), e);
                }
            }
        }

        protected override void ProceedResults(PackResults results)
        {
            try
            {
                var query = new StringBuilder();
                foreach (var result in results.IntegratePackResult)
                {
                    if (result.IsSuccess)
                    {
                        query.AppendLine(String.Format("Update {1} Set Status = 1 Where gateId = '{0}';", result.Id, _tableName));
                    }
                    else
                    {
                        var errorMessage = String.IsNullOrEmpty(result.ErrorMessage) ? String.Empty : result.ErrorMessage.Replace("'", "''").Replace("{", "").Replace("}", "");
                        if (errorMessage.Length > 250) errorMessage = errorMessage.Substring(0, 250);
                        query.AppendLine(String.Format("Update {2} Set Status = 2, ErrorMessage = '{1}' Where gateId = '{0}';", result.Id, errorMessage, _tableName));
                    }
                }

                var sql = query.ToString();
                if (!string.IsNullOrEmpty(sql))
                    DBConnectionProvider.ExecuteNonQuery(sql);
            }
            catch (Exception e)
            {
                Logger.LogError(JsonConvert.SerializeObject(results), e);
            }
        }

        protected override string GetSerializedCollection(List<BaseIntegrationObject> pack)
		{
			return JsonConvert.SerializeObject(pack.Select(p => (ContactTag)p).ToList());
		}
	}
}
