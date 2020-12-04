using FamilIntegrationCore.Models;
using FamilIntegrationService.Models;
using FamilIntegrationService.Providers;
using Newtonsoft.Json;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace ProcessingIntegrationService.Managers
{
	public class CardSynchronizer
	{
		public void SynchronizeCardWithCS() 
		{
			var pack = new Dictionary<Guid, string>();

			do
			{
				pack.Clear();
				using (var conn = new NpgsqlConnection(GetConnectionString()))
				{
					conn.Open();

					using (var cmd = new NpgsqlCommand(@"SELECT ""CardId"", ""Number""
															FROM ""public"".""CardTemp"" Where ""IsSendedToCS"" = 0 and ""AttemptCount"" < 5 limit 100", conn))
					{
						using (var reader = cmd.ExecuteReader())
						{
							while (reader.Read())
							{
								pack.Add(reader.GetGuid(0), reader.GetString(1));
							}
						}
					}
				}
				var cards = pack.Select(c => new CardProcessingModel() { Id = c.Key, ERPId = c.Key.ToString(), CardId = c.Key.ToString(), Number = c.Value, State = 0, IsMain = false, ContactId = String.Empty });
				var body = JsonConvert.SerializeObject(cards);

				var provider = new CSIntegrationProvider(true);
				var result = provider.Request("LoadCardPack", body);

				if (result.IsSuccess)
				{
					var res = JsonConvert.DeserializeObject<List<PackResult>>(result.ResponseStr);
					res.ForEach(r => { if (r.IsSuccess) SetCardSended(new Guid(r.Id)); else SetCardError(new Guid(r.Id), r.ErrorMessage); });
				}
				else 
				{
					cards.ToList().ForEach(c => SetCardError(c.Id, result.ResponseStr));
				}

			} while (pack.Count > 0);
		}

		internal void CleanCardTempTable()
		{
			using (var conn = new NpgsqlConnection(GetConnectionString()))
			{
				conn.Open();

				using (var cmd = new NpgsqlCommand(@"delete FROM ""public"".""CardTemp"" Where ""IsSendedToBPM"" = 1 and ""IsSendedToCS"" = 1;
														Update  ""public"".""CardTemp"" Set ""AttemptCount"" = 4 Where ""AttemptCount"" = 5;", conn))
				{
					cmd.ExecuteNonQuery();
				}
			}
		}

		public void SynchronizeCardWithBPM()
		{
			var pack = new Dictionary<Guid, string>();

			do
			{
				pack.Clear();
				using (var conn = new NpgsqlConnection(GetConnectionString()))
				{
					conn.Open();

					using (var cmd = new NpgsqlCommand(@"SELECT ""CardId"", ""Number""
															FROM ""public"".""CardTemp"" Where ""IsSendedToBPM"" = 0 and ""AttemptCount"" < 5 limit 100", conn))
					{
						using (var reader = cmd.ExecuteReader())
						{
							while (reader.Read())
							{
								pack.Add(reader.GetGuid(0), reader.GetString(1));
							}
						}
					}
				}
				var cards = pack.Select(c => new Card() { Id = c.Key, Number = c.Value, CardStatus = "1", IsMain = false });
				var body = JsonConvert.SerializeObject(cards);

				var provider = new CRMIntegrationProvider();
				body = String.Format(@"{{""request"": {0}}}", new IntegrationObjectRequest() { Objects = body, TableName = "CardGenerate" }.ToJson());
				var result = provider.MakeRequest("GateIntegrationService/IntegratePack", body);

				if (result.IsSuccess)
				{
					//var results = JsonConvert.DeserializeObject<PackResults>(result.ResponseStr);
					//results.IntegratePackResult.ForEach(r => { if (r.IsSuccess) SetCardSended(new Guid(r.Id)); else SetCardError(new Guid(r.Id), r.ErrorMessage); });
					cards.ToList().ForEach(c => SetCardSended(c.Id, "IsSendedToBPM"));
				}
				else
				{
					cards.ToList().ForEach(c => SetCardError(c.Id, result.ResponseStr, "IsSendedToBPM"));
				}

			} while (pack.Count > 0);
		}

		private void SetCardSended(Guid cardId, string fieldName = "IsSendedToCS")
		{
			using (var conn = new NpgsqlConnection(GetConnectionString()))
			{
				conn.Open();

				using (var cmd = new NpgsqlCommand($@"Update ""public"".""CardTemp"" Set ""{fieldName}"" = 1 Where ""CardId"" = '{cardId}'", conn))
				{
					cmd.ExecuteNonQuery();
				}
			}
		}

		private void SetCardError(Guid cardId, string error, string fieldName = "IsSendedToCS")
		{
			using (var conn = new NpgsqlConnection(GetConnectionString()))
			{
				conn.Open();

				using (var cmd = new NpgsqlCommand($@"Update ""public"".""CardTemp"" Set ""{fieldName}"" = 0, ""AttemptCount"" = ""AttemptCount"" + 1, ""LastError"" = '{error.Replace("'", "''")}' Where ""CardId"" = '{cardId}'", conn))
				{
					cmd.ExecuteNonQuery();
				}
			}
		}

		private static string GetConnectionString()
		{
			GlobalCacheReader.GetValue(GlobalCacheReader.CacheKeys.ConnectionString, out string connString);
			return connString;
		}
	}
}
