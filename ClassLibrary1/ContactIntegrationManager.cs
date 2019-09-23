using FamilIntegrationService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using Terrasoft.Core;
using Terrasoft.Core.DB;
using Newtonsoft.Json;

namespace Terrasoft.Configuration
{
	public class ProductSizeIntegrationManager
	{
		private LookupManager _lookupManager;
		private string _tableName = "SmrProductGroup";

		public UserConnection UserConnection { get; private set; }

		public ProductSizeIntegrationManager(UserConnection uc)
		{
			UserConnection = uc;
			_lookupManager = new LookupManager(uc);
		}

		public List<PackResult> IntegrateProductGroupPack(string request)
		{
			var productGroups = JsonConvert.DeserializeObject<List<ProductGroupGateInfo>>(request);
			var lookupManager = new LookupManager(UserConnection);
			var result = new List<PackResult>();
			foreach (var info in productGroups)
			{
				try
				{
					var categoryId = lookupManager.FindLookupIdByName(info.Category, "ProductType");

					if (categoryId == null)
					{
						result.Add(new PackResult()
						{
							IsSuccess = false,
							Id = info.ERPId,
							ErrorMessage = "Подкатегория с указанным названием не найдена в bpm’online"
						});

						continue;
					}

					var lookupId = lookupManager.FindLookupId(productGateInfo.ERPId, _tableName);
					if (lookupId == Guid.Empty)
					{
						GetInsertQuery(productGateInfo).Execute();
						lookupManager.AddToLookup(productGateInfo, _tableName);
					}
					else
					{
						GetUpdateQuery(productGateInfo).Execute();
						lookupManager.UpdateLookup(productGateInfo, _tableName);
					}

					result.Add(new PackResult()
					{
						IsSuccess = true,
						Id = productGateInfo.ERPId
					});
				}
				catch (Exception e)
				{
					result.Add(new PackResult()
					{
						IsSuccess = false,
						Id = productGateInfo.ERPId,
						ErrorMessage = e.Message
					});
				}
			}
			return result;
		}

		public PackResult PrimaryIntegrateProductSizePack(string request)
		{
			var objs = JsonConvert.DeserializeObject<List<SimpleLookupGateInfo>>(request);
			var lookupManager = new LookupManager(UserConnection);
			var result = new List<PackResult>();
			var sb = new StringBuilder();
			sb.AppendLine(GetInsertHead());
			var values = new List<string>();
			foreach (var info in objs)
			{
				values.Add(GetInsertValues(info));
			}

			sb.AppendLine(string.Join(",", values));
			new CustomQuery(UserConnection, sb.ToString()).Execute();
			return new PackResult()
			{
				IsSuccess = true,
			};
		}

		private string GetInsertHead()
		{
			return "Insert into SmrProductSize(Name, SmrERPId, Id) VALUES";
		}

		public string GetInsertValues(SimpleLookupGateInfo info)
		{
			return String.Format("('{0}', '{1}', '{2}')", info.Name, info.ERPId, info.Id);
		}

		private Insert GetInsertQuery(SimpleLookupGateInfo info)
		{
			return new Insert(UserConnection)
						.Into(_tableName)
						.Set("Name", Column.Parameter(info.Name))
						.Set("Id", Column.Parameter(info.Id))
						.Set("SmrERPId", Column.Parameter(info.ERPId)) as Insert;
		}

		private Update GetUpdateQuery(SimpleLookupGateInfo info)
		{
			var update = new Update(UserConnection, "SmrProductSize")
						.Set("Name", Column.Parameter(info.Name))
						as Update;

			update.Where("SmrERPId").IsEqual(Column.Parameter(info.ERPId));

			return update;
		}
	}
}
