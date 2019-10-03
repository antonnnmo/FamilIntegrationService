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
		protected object _lockRes = new object();

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

						if (!res.IsSuccess)
						{
							ProceedResult(new PackResult() { IsSuccess = false, ErrorMessage = res.ResponseStr }, pack);
						}
						else
						{
							var results = JsonConvert.DeserializeObject<PackResults>(res.ResponseStr);

							if (_isNeedSendToProcessing)
							{
								var successResults = results.IntegratePackResult.Where(r => r.IsSuccess);
								var unsuccessResults = results.IntegratePackResult.Where(r => !r.IsSuccess);
                                foreach (var r in successResults)
                                {
                                    var p = pack.FirstOrDefault(x => x.ERPId == r.Id);
                                    if (p != null && r.CustomFields != null) p.CustomFields = r.CustomFields;
                                }
								var processingResults = SendToProcessing(pack.Where(p => successResults.FirstOrDefault(r => r.Id == p.ERPId) != null).ToList());

								results = new PackResults();

								if (processingResults.IsSuccess)
								{
									results.IntegratePackResult = unsuccessResults.Union(JsonConvert.DeserializeObject<List<PackResult>>(processingResults.ResponseStr)).ToList();
									ProceedResults(results);
								}
								else
								{
									SetProcessingErrors(pack, processingResults.ResponseStr);
									ProceedResults(new PackResults() { IntegratePackResult = unsuccessResults.ToList() });
								}
							}
							else
							{
								ProceedResults(results);
							}
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

		private void SetProcessingErrors(List<BaseIntegrationObject> pack, string responseStr)
		{
			var errorMessage = String.IsNullOrEmpty(responseStr) ? String.Empty : responseStr.Replace("'", "''").Replace("{", "").Replace("}", "");
			if (errorMessage.Length > 250) errorMessage = errorMessage.Substring(0, 250);
			DBConnectionProvider.ExecuteNonQuery(String.Format("Update {1} Set Status = 2, ErrorMessage = '{2}' Where ERPId in ({0})", String.Join(",", pack.Select(p => String.Format("'{0}'", p.ERPId))), _tableName, errorMessage));
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
						RequestResult res = null;
						try
						{
							res = crm.MakeRequest("GateIntegrationService/PrimaryIntegratePack", GetBody(pack));
							if (!res.IsSuccess)
							{
								ProceedResult(new PackResult() { IsSuccess = false, ErrorMessage = res.ResponseStr }, pack);
							}
							else
							{
                                var results = JsonConvert.DeserializeObject<PrimaryIntegratePackResponse>(res.ResponseStr);
                                if (_isNeedSendToProcessing)
                                {
                                    var successResults = results.PrimaryIntegratePackResult.Where(r => r.IsSuccess);
                                    var unsuccessResults = results.PrimaryIntegratePackResult.Where(r => !r.IsSuccess);
                                    foreach (var r in successResults)
                                    {
                                        var p = pack.FirstOrDefault(x => x.ERPId == r.Id);
                                        if (p != null && r.CustomFields != null) p.CustomFields = r.CustomFields;
                                    }
                                    var processingResults = SendToProcessingPrimary(pack.Where(p => successResults.FirstOrDefault(r => r.Id == p.ERPId) != null).ToList());

                                    results = new PrimaryIntegratePackResponse();

                                    if (processingResults.IsSuccess)
                                    {
                                        results.PrimaryIntegratePackResult = unsuccessResults.Union(JsonConvert.DeserializeObject<List<PackResult>>(processingResults.ResponseStr)).ToList();
                                        ProceedPrimaryResults(results);
                                    }
                                    else
                                    {
                                        SetProcessingErrors(pack, processingResults.ResponseStr);
                                        ProceedResults(new PackResults() { IntegratePackResult = unsuccessResults.ToList() });
                                    }
                                }
                                else
                                {
                                    ProceedPrimaryResults(results);
                                }
                                /*var result = JsonConvert.DeserializeObject<PrimaryIntegratePackResponse>(res.ResponseStr);

								if (result.PrimaryIntegratePackResult.IsSuccess && _isNeedSendToProcessing)
								{
									var isProcessLoadSuccess = SendToProcessingPrimary(pack).IsSuccess;
									ProceedResult(new PackResult() { IsSuccess = isProcessLoadSuccess }, pack);
								}
								else
								{
									ProceedResult(result.PrimaryIntegratePackResult, pack);
								}*/
                            }
						}
						catch (Exception e)
						{
							ProceedResult(new PackResult() { IsSuccess = false, ErrorMessage = $"{e.Message} {res}"}, pack);
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

        protected string GetCustomFieldsValue(BaseIntegrationObject o, string field)
        {
            if (o.CustomFields == null) return string.Empty;
            var res = o.CustomFields.FirstOrDefault(f => f.Name == field);
            if (res != null) return res.Value ?? string.Empty;
            return string.Empty;
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

        protected void ProceedPrimaryResults(PrimaryIntegratePackResponse results)
        {
            try
            {
                var query = new StringBuilder();
                foreach (var result in results.PrimaryIntegratePackResult)
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
			lock (_lockRes)
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
						var errorMessage = String.IsNullOrEmpty(result.ErrorMessage) ? String.Empty : result.ErrorMessage.Replace("'", "").Replace("{", "").Replace("}", "");
						if (errorMessage.Length > 250) errorMessage = errorMessage.Substring(0, 250);
						foreach (var obj in pack)
						{
							query.AppendLine(String.Format("Update {2} Set Status = 2, ErrorMessage = '{1}' Where ERPId = '{0}';", obj.ERPId, errorMessage, _tableName));
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
}
