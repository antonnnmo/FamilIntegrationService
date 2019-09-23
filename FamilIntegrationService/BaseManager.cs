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
	public class BaseManager
	{
		protected int packSize = 500;
		protected int threadCount = 20;
		protected string _tableName;
		protected string _processingPrimaryMethodName;
		protected string _processingMethodName;
		protected bool _isNeedSendToProcessing;
		protected object _lock = new object();

		protected virtual List<BaseIntegrationObject> ReadPack()
		{
			return new List<BaseIntegrationObject>();
		}

		public virtual void Execute()
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
						var crm = new CRMIntegrationProvider(true);
						var res = crm.MakeRequest("GateIntegrationService/IntegratePack", GetBody(pack));
						var results = JsonConvert.DeserializeObject<PackResults>(res);

						if (_isNeedSendToProcessing)
						{
							var successResults = results.IntegratePackResult.Where(r => r.IsSuccess);
							var unsuccessResults = results.IntegratePackResult.Where(r => !r.IsSuccess);
							var processingResults = SendToProcessing(pack.Where(p => successResults.FirstOrDefault(r => r.Id == p.ERPId) != null).ToList());

							results = new PackResults();

							if (processingResults.IsSuccess)
							{
								results.IntegratePackResult = unsuccessResults.Union(JsonConvert.DeserializeObject<List<PackResult>>(processingResults.ResponseStr)).ToList();
								ProceedResults(results);
							}
							else
							{
								SetProcessingErrors(pack);
							}
						}
						else
						{
							ProceedResults(results);
						}

						Logger.LogInfo(_tableName, "pack finished");
						pack = ReadPack();
					}
				});

				task.Start();
				tasks.Add(task);
			}

			Task.WaitAll(tasks.ToArray());

			Logger.LogInfo("Finished", _tableName);
		}

		private void SetProcessingErrors(List<BaseIntegrationObject> pack)
		{
			DBConnectionProvider.ExecuteNonQuery(String.Format("Update {1} Set Status = 3 Where ERPId in ({0})", String.Join(",", pack.Select(p => String.Format("'{0}'", p.ERPId))), _tableName));
		}

		private RequestResult SendToProcessing(List<BaseIntegrationObject> pack)
		{
			var processingIntegrationProvider = new ProcessingIntegrationProvider();
			return processingIntegrationProvider.Request(_processingMethodName, GetProcessingPackBody(pack));
		}

		public virtual void ExecutePrimary()
		{
			Logger.LogInfo("Начался первичный импорт", _tableName);

			var tasks = new List<Task>();
			for (var j = 0; j < threadCount; j++)
			{
				var task = new Task(() =>
				{
					var pack = ReadPack();
					while (pack.Count > 0)
					{
						var crm = new CRMIntegrationProvider(true);
						var res = crm.MakeRequest("GateIntegrationService/PrimaryIntegratePack", GetBody(pack));
						var result = JsonConvert.DeserializeObject<PrimaryIntegratePackResponse>(res);

						if (result.PrimaryIntegratePackResult.IsSuccess && _isNeedSendToProcessing)
						{
							var isProcessLoadSuccess = SendToProcessingPrimary(pack).IsSuccess;
							ProceedResult(new PackResult() { IsSuccess = isProcessLoadSuccess }, pack);
						}
						else
						{
							ProceedResult(result.PrimaryIntegratePackResult, pack);
						}

						Logger.LogInfo(_tableName, "pack finished");
						pack = ReadPack();
					}
				});

				task.Start();
				tasks.Add(task);
			}
		
			Task.WaitAll(tasks.ToArray());

			Logger.LogInfo("Finished", _tableName);
		}

		//public void ExecutePrimary()
		//{
		//	Logger.LogInfo("Начался первичный импорт", _tableName);

		//	int i = 0;
		//	while (ReadPack())
		//	{
		//		Logger.LogInfo("Началась обработка пачки:", ((i++) * packSize).ToString());
		//		var threadPacks = new List<List<BaseIntegrationObject>>();
		//		for (var j = 0; j < threadCount; j++)
		//		{
		//			threadPacks.Add(GetIntegrationObjectCollection().Skip(j * (packSize / threadCount)).Take(packSize / threadCount).ToList());
		//		}

		//		var tasks = new List<Task>();
		//		foreach (var pack in threadPacks)
		//		{
		//			var task = new Task(() =>
		//			{
		//				var crm = new CRMIntegrationProvider(true);
		//				var res = crm.MakeRequest("GateIntegrationService/PrimaryIntegratePack", GetBody(pack));
		//				var result = JsonConvert.DeserializeObject<PrimaryIntegratePackResponse>(res);

		//				if (result.PrimaryIntegratePackResult.IsSuccess && _isNeedSendToProcessing)
		//				{
		//					var isProcessLoadSuccess = SendToProcessingPrimary(pack).IsSuccess;
		//					ProceedResult(new PackResult() { IsSuccess = isProcessLoadSuccess }, pack);
		//				}
		//				else
		//				{
		//					ProceedResult(result.PrimaryIntegratePackResult, pack);
		//				}

		//				Logger.LogInfo(_tableName, "pack finished");
		//			});

		//			task.Start();
		//			tasks.Add(task);
		//		}

		//		Task.WaitAll(tasks.ToArray());
		//	}

		//	Logger.LogInfo("Finished", _tableName);
		//}

		protected string GetBody(List<BaseIntegrationObject> pack)
		{
			return String.Format(@"{{""request"": {0}}}", new IntegrationObjectRequest() { Objects = GetSerializedCollection(pack), TableName = _tableName }.ToJson());
		}

		protected virtual string GetSerializedCollection(List<BaseIntegrationObject> pack)
		{
			return String.Empty;
		}

		protected virtual string GetProcessingPackBody(List<BaseIntegrationObject> pack) { return String.Empty; }

		protected RequestResult SendToProcessingPrimary(List<BaseIntegrationObject> pack)
		{
			var processingIntegrationProvider = new ProcessingIntegrationProvider();
			return processingIntegrationProvider.Request(_processingPrimaryMethodName, GetProcessingPackBody(pack));
		}

		protected void ProceedResults(PackResults results)
		{
			try
			{
				var query = new StringBuilder();
				foreach (var result in results.IntegratePackResult)
				{
					if (result.IsSuccess)
					{
						query.AppendLine(String.Format("Update {1} Set Status = 1 Where ERPId = '{0}';", result.Id, _tableName));
					}
					else
					{
						var errorMessage = String.IsNullOrEmpty(result.ErrorMessage) ? String.Empty : result.ErrorMessage.Replace("'", "''").Replace("{", "").Replace("}", "");
						if (errorMessage.Length > 250) errorMessage = errorMessage.Substring(0, 250);
						query.AppendLine(String.Format("Update {2} Set Status = 2, ErrorMessage = '{1}' Where ERPId = '{0}';", result.Id, errorMessage, _tableName));
					}
				}

				DBConnectionProvider.ExecuteNonQuery(query.ToString());
			}
			catch (Exception e)
			{
				Logger.LogError(JsonConvert.SerializeObject(results), e);
			}
		}

		public void ProceedResult(PackResult result, List<BaseIntegrationObject> pack)
		{
			try
			{
				var query = new StringBuilder();

				if (result.IsSuccess)
				{
					foreach (var obj in pack)
					{
						query.AppendLine(String.Format("Update {1} Set Status = 1 Where ERPId = '{0}';", obj.ERPId, _tableName));
					}
				}
				else
				{
					foreach (var obj in pack)
					{
						query.AppendLine(String.Format("Update {2} Set Status = 2, ErrorMessage = '{1}' Where ERPId = '{0}';", obj.ERPId, result.ErrorMessage, _tableName));
					}
				}

				DBConnectionProvider.ExecuteNonQuery(query.ToString());
			}
			catch (Exception e)
			{
				Logger.LogError(String.Format("Ошибка обновления состояний в ШТ {0} для первичного импорта", _tableName), e);
			}
		}
	}
}
