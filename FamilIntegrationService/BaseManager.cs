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
		protected bool _isNeedSendToPersonalArea;
		protected object _lock = new object();
		protected object _lockRes = new object();

        public BaseManager()
        {
            GlobalCacheReader.GetValue<int>(GlobalCacheReader.CacheKeys.PackSize, out packSize);
            GlobalCacheReader.GetValue<int>(GlobalCacheReader.CacheKeys.ThreadCount, out threadCount);
        }

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
                    var pack = new List<BaseIntegrationObject>();
                    try
                    {
                        pack = ReadPack();
                        Logger.LogInfo(string.Format("Прочитано данных из {0}: {1}", _tableName, pack.Count), "");
                    }
                    catch(Exception e)
                    {
                        Logger.LogError(string.Format("Ошибка чтения данных из {0}", _tableName), e);
                    }

					var crm = new CRMIntegrationProvider(true);

					while (pack.Count > 0)
					{
                        var now = DateTime.Now;

						var res = crm.MakeRequest("GateIntegrationService/IntegratePack", GetBody(pack));

                        Logger.LogInfo(string.Format("Запрос {0} к CRM выполнен за {1}с", _tableName, (DateTime.Now - now).TotalSeconds.ToString("F1")), "");

                        if (!res.IsSuccess)
						{
							ProceedResult(new PackResult() { IsSuccess = false, ErrorMessage = res.ResponseStr }, pack);
						}
						else
						{
							var results = JsonConvert.DeserializeObject<PackResults>(res.ResponseStr);

							var successResults = results.IntegratePackResult.Where(r => r.IsSuccess);
							var unsuccessResults = results.IntegratePackResult.Where(r => !r.IsSuccess);
							ProceedResults(new PackResults() { IntegratePackResult = unsuccessResults.ToList() });

							foreach (var r in successResults)
							{
								var p = pack.FirstOrDefault(x => x.ERPId == r.Id);
								if (p != null && r.CustomFields != null) p.CustomFields = r.CustomFields;
							}

							if (_isNeedSendToProcessing)
							{
								var isProcessingSuccess = false;
								try
								{
									isProcessingSuccess = SendToProcessing(pack, out now, out results, successResults, false);
								}
								catch (Exception e)
								{
									Logger.LogError("SendToProcessing error ", e);
								}

								successResults = results.IntegratePackResult.Where(r => r.IsSuccess);
								unsuccessResults = results.IntegratePackResult.Where(r => !r.IsSuccess);

								ProceedResults(new PackResults() { IntegratePackResult = unsuccessResults.ToList() });

								if (_isNeedSendToPersonalArea)
								{
									var isPersonalAreaSuccess = SendToProcessing(pack, out now, out results, successResults, true);

									if (isPersonalAreaSuccess)
									{
										ProceedResults(results);
									}
								}
								else
								{
									ProceedResults(results);
								}
							}
							else
							{
								ProceedResults(results);
							}
						}

						Logger.LogInfo(_tableName, "pack finished");
                        pack = new List<BaseIntegrationObject>();
                        try
                        {
                            pack = ReadPack();
                            Logger.LogInfo(string.Format("Прочитано данных из {0}: {1}", _tableName, pack.Count), "");
                        }
                        catch (Exception e)
                        {
                            Logger.LogError(string.Format("Ошибка чтения данных из {0}", _tableName), e);
                        }
                    }
				});

				task.Start();
				tasks.Add(task);
			}

			Task.WaitAll(tasks.ToArray());

			Logger.LogInfo("Finished", _tableName);
		}

		private bool SendToProcessing(List<BaseIntegrationObject> pack, out DateTime now, out PackResults results, IEnumerable<PackResult> successResults, bool isUsePA)
		{
			now = DateTime.Now;

			var processingResults = SendToProcessing(pack.Where(p => successResults.FirstOrDefault(r => r.Id == p.ERPId) != null).ToList(), isUsePA, _processingMethodName);

			Logger.LogInfo(string.Format("Запрос {0} к процессингу выполнен за {1}с", _tableName, (DateTime.Now - now).TotalSeconds.ToString("F1")), "");

			results = new PackResults();

			if (processingResults.IsSuccess)
			{
				results.IntegratePackResult = JsonConvert.DeserializeObject<List<PackResult>>(processingResults.ResponseStr);
				return true;
			}
			else
			{
				SetProcessingErrors(pack, processingResults.ResponseStr);
				return false;
			}
		}

		private bool SendToProcessingPrimary(List<BaseIntegrationObject> pack, DateTime now, out PrimaryIntegratePackResponse results, IEnumerable<PackResult> successResults, bool isUsePA)
		{
			now = DateTime.Now;

			var processingResults = SendToProcessing(pack.Where(p => successResults.FirstOrDefault(r => r.Id == p.ERPId) != null).ToList(), isUsePA, _processingPrimaryMethodName);

			Logger.LogInfo(string.Format("Запрос {0} к процессингу выполнен за {1}с", _tableName, (DateTime.Now - now).TotalSeconds.ToString("F1")), "");

			results = new PrimaryIntegratePackResponse();

			if (processingResults.IsSuccess)
			{
				try
				{
					results.PrimaryIntegratePackResult = JsonConvert.DeserializeObject<List<PackResult>>(processingResults.ResponseStr);
					return true;
				}
				catch (Exception e)
				{
					ProceedResult(new PackResult() { IsSuccess = false, ErrorMessage = $"{e.Message} {processingResults.ResponseStr}" }, pack);
					Logger.LogInfo(processingResults.ResponseStr, "");
					return false;
				}
			}
			else
			{
				SetProcessingErrors(pack, processingResults.ResponseStr);
				return false;
			}
		}

		private RequestResult SendToProcessing(List<BaseIntegrationObject> pack, bool isUsePA, string methodName)
		{
			var processingIntegrationProvider = new ProcessingIntegrationProvider(isUsePA);
			return processingIntegrationProvider.Request(methodName, GetProcessingPackBody(pack));
		}

		private void SetProcessingErrors(List<BaseIntegrationObject> pack, string responseStr)
		{
			var errorMessage = String.IsNullOrEmpty(responseStr) ? String.Empty : responseStr.Replace("'", "''").Replace("{", "").Replace("}", "");
			if (errorMessage.Length > 250) errorMessage = errorMessage.Substring(0, 250);
			DBConnectionProvider.ExecuteNonQuery(String.Format("Update {1} Set Status = 2, ErrorMessage = '{2}' Where ERPId in ({0})", String.Join(",", pack.Select(p => String.Format("'{0}'", p.CorrectERPId))), _tableName, errorMessage));
		}

		public virtual void ExecutePrimary()
		{
			Logger.LogInfo("Начался первичный импорт", _tableName);

			var tasks = new List<Task>();
			for (var j = 0; j < threadCount; j++)
			{
				var task = new Task(() =>
				{
                    var pack = new List<BaseIntegrationObject>();
                    try
                    {
                        pack = ReadPack();
                        Logger.LogInfo(string.Format("Прочитано данных из {0}: {1}", _tableName, pack.Count), "");
                    }
                    catch (Exception e)
                    {
                        Logger.LogError(string.Format("Ошибка чтения данных из {0}", _tableName), e);
                    }

					var crm = new CRMIntegrationProvider(true);

					while (pack.Count > 0)
					{
						RequestResult res = null;
						try
						{
                            var now = DateTime.Now;

                            res = crm.MakeRequest("GateIntegrationService/PrimaryIntegratePack", GetBody(pack));

                            Logger.LogInfo(string.Format("Запрос {0} к CRM выполнен за {1}с", _tableName, (DateTime.Now - now).TotalSeconds.ToString("F1")), "");

                            if (!res.IsSuccess)
							{
								ProceedResult(new PackResult() { IsSuccess = false, ErrorMessage = res.ResponseStr }, pack);
							}
							else
							{
                                var results = JsonConvert.DeserializeObject<PrimaryIntegratePackResponse>(res.ResponseStr);
								var successResults = results.PrimaryIntegratePackResult.Where(r => r.IsSuccess);
								var unsuccessResults = results.PrimaryIntegratePackResult.Where(r => !r.IsSuccess);

								ProceedPrimaryResults(new PrimaryIntegratePackResponse() { PrimaryIntegratePackResult = unsuccessResults.ToList() });

								foreach (var r in successResults)
								{
									var p = pack.FirstOrDefault(x => x.ERPId == r.Id);
									if (p != null && r.CustomFields != null) p.CustomFields = r.CustomFields;
								}

								if (_isNeedSendToProcessing)
								{
									var isProcessingResultSuccess = SendToProcessingPrimary(pack, now, out results, successResults, false);

									if (isProcessingResultSuccess)
									{
										if (_isNeedSendToPersonalArea)
										{
											var isPersonalAreaResultSuccess = SendToProcessingPrimary(pack, now, out results, successResults, true);
											if (isPersonalAreaResultSuccess)
											{
												ProceedPrimaryResults(results);
											}
										}
										else
										{
											ProceedPrimaryResults(results);
										}
									}
								}
								else
                                {
                                    ProceedPrimaryResults(results);
                                }
                            }
						}
						catch (Exception e)
						{
							ProceedResult(new PackResult() { IsSuccess = false, ErrorMessage = $"{e.Message} {res}"}, pack);
						}

						Logger.LogInfo(_tableName, "pack finished");
                        pack = new List<BaseIntegrationObject>();
                        try
                        {
                            pack = ReadPack();
                            Logger.LogInfo(string.Format("Прочитано данных из {0}: {1}", _tableName, pack.Count), "");
                        }
                        catch (Exception e)
                        {
                            Logger.LogError(string.Format("Ошибка чтения данных из {0}", _tableName), e);
                        }
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

		

        protected string GetCustomFieldsValue(BaseIntegrationObject o, string field)
        {
            if (o.CustomFields == null) return string.Empty;
            var res = o.CustomFields.FirstOrDefault(f => f.Name == field);
            if (res != null) return res.Value ?? string.Empty;
            return string.Empty;
        }

        protected Guid? GetCustomFieldsGuidValue(BaseIntegrationObject o, string field)
        {
            var guid = Guid.Empty;
            if (o.CustomFields == null) return null;
            var res = o.CustomFields.FirstOrDefault(f => f.Name == field);
            if (res != null)
            {
                if (Guid.TryParse(res.Value ?? string.Empty, out guid))
                    return guid;
            }
            return null;
        }

        protected virtual void ProceedResults(PackResults results)
		{
            try
			{
				var query = new StringBuilder();
				foreach (var result in results.IntegratePackResult)
				{
					if (result.IsSuccess)
					{
						query.AppendLine(String.Format("Update {1} Set Status = 1 Where ERPId = '{0}';", result.GetCorrectId(), _tableName));
					}
					else
					{
						var errorMessage = String.IsNullOrEmpty(result.ErrorMessage) ? String.Empty : result.ErrorMessage.Replace("'", "''").Replace("{", "").Replace("}", "");
						if (errorMessage.Length > 250) errorMessage = errorMessage.Substring(0, 250);
						query.AppendLine(String.Format("Update {2} Set Status = 2, ErrorMessage = '{1}' Where ERPId = '{0}';", result.GetCorrectId(), errorMessage, _tableName));
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

        protected void ProceedPrimaryResults(PrimaryIntegratePackResponse results)
        {
            try
            {
                var query = new StringBuilder();
                foreach (var result in results.PrimaryIntegratePackResult)
                {
                    if (result.IsSuccess)
                    {
                        query.AppendLine(String.Format("Update {1} Set Status = 1 Where ERPId = '{0}';", result.GetCorrectId(), _tableName));
                    }
                    else
                    {
                        var errorMessage = String.IsNullOrEmpty(result.ErrorMessage) ? String.Empty : result.ErrorMessage.Replace("'", "''").Replace("{", "").Replace("}", "");
                        if (errorMessage.Length > 250) errorMessage = errorMessage.Substring(0, 250);
                        query.AppendLine(String.Format("Update {2} Set Status = 2, ErrorMessage = '{1}' Where ERPId = '{0}';", result.GetCorrectId(), errorMessage, _tableName));
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

        public virtual void ProceedResult(PackResult result, List<BaseIntegrationObject> pack)
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
							query.AppendLine(String.Format("Update {1} Set Status = 1 Where ERPId = '{0}';", obj.CorrectERPId, _tableName));
						}
					}
					else
					{
						var errorMessage = String.IsNullOrEmpty(result.ErrorMessage) ? String.Empty : result.ErrorMessage.Replace("'", "").Replace("{", "").Replace("}", "");
						if (errorMessage.Length > 250) errorMessage = errorMessage.Substring(0, 250);
						foreach (var obj in pack)
						{
							query.AppendLine(String.Format("Update {2} Set Status = 2, ErrorMessage = '{1}' Where ERPId = '{0}';", obj.CorrectERPId, errorMessage, _tableName));
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
	}
}
