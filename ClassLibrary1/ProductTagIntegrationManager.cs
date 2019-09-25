using FamilIntegrationService.Models;
using System;
using System.Collections.Generic;
using System.Text;
using Terrasoft.Core;
using Terrasoft.Core.DB;
using Newtonsoft.Json;

namespace Terrasoft.Configuration
{
	public class ProductIntegrationManager
	{
		private static readonly string ERPDataSourceId = "27F9DE70-4B0F-423C-AC62-C4730414F3B3";
		private LookupManager _lookupManager;
		private string _tableName = "Product";

		public UserConnection UserConnection { get; private set; }

		public ProductIntegrationManager(UserConnection uc)
		{
			UserConnection = uc;
			_lookupManager = new LookupManager(uc);
		}

		public List<PackResult> IntegratePack(string request)
		{
			var objs = JsonConvert.DeserializeObject<List<ProductGateInfo>>(request);
			var lookupManager = new LookupManager(UserConnection);
			var result = new List<PackResult>();
			foreach (var info in objs)
			{
				try
				{
					var directionId = lookupManager.FindLookupIdByName(info.Direction, "SmrProductDirection");
					if (directionId == null)
					{
						result.Add(new PackResult()
						{
							IsSuccess = false,
							Id = info.ERPId,
							ErrorMessage = "Дирекция продукта не найдена среди существующих в bpm’online"
						});

						continue;
					}

					var categoryId = lookupManager.FindLookupIdByName(info.Category, "ProductCategory");
					if (categoryId == null)
					{
						result.Add(new PackResult()
						{
							IsSuccess = false,
							Id = info.ERPId,
							ErrorMessage = "Категория продукта не найдена среди существующих в bpm’online"
						});

						continue;
					}

					var subCategoryId = lookupManager.FindLookupIdByName(info.SubCategory, "ProductType");
					if (subCategoryId == null)
					{
						result.Add(new PackResult()
						{
							IsSuccess = false,
							Id = info.ERPId,
							ErrorMessage = "Подкатегория продукта не найдена среди существующих в bpm’online"
						});

						continue;
					}

					var groupId = lookupManager.FindLookupIdByName(info.Group, "SmrProductGroup");
					if (groupId == null)
					{
						result.Add(new PackResult()
						{
							IsSuccess = false,
							Id = info.ERPId,
							ErrorMessage = "Группа продукта не найдена среди существующих в bpm’online"
						});

						continue;
					}

					var sizeId = lookupManager.FindLookupIdByName(info.Size, "SmrProductSize");
					if (sizeId == null)
					{
						result.Add(new PackResult()
						{
							IsSuccess = false,
							Id = info.ERPId,
							ErrorMessage = "Размер продукта не найден среди существующих в bpm’online"
						});

						continue;
					}

					var tradeMarkId = lookupManager.FindLookupIdByName(info.Brand, "Trademark");
					if (tradeMarkId == null)
					{
						result.Add(new PackResult()
						{
							IsSuccess = false,
							Id = info.ERPId,
							ErrorMessage = "Бренд продукта не найден среди существующих в bpm’online"
						});

						continue;
					}

					var select = new Select(UserConnection).Top(1).Column("Id").Column("Code").From(_tableName).Where("SmrERPId").IsEqual(Column.Parameter(info.ERPId)) as Select;
					var objId = Guid.Empty;
					var code = String.Empty;

					using (var dbExecutor = UserConnection.EnsureDBConnection())
					{
						using (var reader = select.ExecuteReader(dbExecutor))
						{
							if (reader.Read())
							{
								objId = reader.GetValue("Id", Guid.Empty);
								code = reader.GetValue("Code", String.Empty);
							}
						}
					}

					if (objId == Guid.Empty)
					{
						GetInsertQuery(info, directionId, categoryId, subCategoryId, groupId, sizeId, tradeMarkId).Execute();
					}
					else
					{
						if (!String.IsNullOrEmpty(code))
						{
							result.Add(new PackResult()
							{
								IsSuccess = false,
								Id = info.ERPId,
								ErrorMessage = "Продукт с таким кодом уже существует в bpm’online"
							});
							continue;
						}
						GetUpdateQuery(info, directionId, categoryId, subCategoryId, groupId, sizeId, tradeMarkId).Execute();
					}

					result.Add(new PackResult()
					{
						IsSuccess = true,
						Id = info.ERPId
					});
				}
				catch (Exception e)
				{
					result.Add(new PackResult()
					{
						IsSuccess = false,
						Id = info.ERPId,
						ErrorMessage = e.Message
					});
				}
			}
			return result;
		}

		private Guid GetContactId(string contactId)
		{
			return (new Select(UserConnection).Column("Id").From("Contact").Where("SmrERPId").IsEqual(Column.Parameter(contactId)) as Select).ExecuteScalar<Guid>();
		}

		public PackResult PrimaryIntegratePack(string request)
		{
			var objs = JsonConvert.DeserializeObject<List<ProductGateInfo>>(request);
			var lookupManager = new LookupManager(UserConnection);
			var result = new List<PackResult>();
			var sb = new StringBuilder();
			sb.AppendLine(GetInsertHead());
			var values = new List<string>();
			foreach (var info in objs)
			{
				var directionId = lookupManager.FindLookupIdByName(info.Direction, "SmrProductDirection");
				var categoryId = lookupManager.FindLookupIdByName(info.Category, "ProductCategory");
				var subCategoryId = lookupManager.FindLookupIdByName(info.SubCategory, "ProductType");
				var groupId = lookupManager.FindLookupIdByName(info.Group, "SmrProductGroup");
				var sizeId = lookupManager.FindLookupIdByName(info.Size, "SmrProductSize");
				var tradeMarkId = lookupManager.FindLookupIdByName(info.Brand, "Trademark");

				values.Add(GetInsertValues(info, directionId, categoryId, subCategoryId, groupId, sizeId, tradeMarkId));
			}

			sb.AppendLine(string.Join(",", values));
			new CustomQuery(UserConnection, sb.ToString()).Execute();
			return new PackResult()
			{
				IsSuccess = true,
			};
		}

		private Guid GetObjectId(string erpId)
		{
			return (new Select(UserConnection).Top(1).Column("Id").From(_tableName).Where("SmrERPId").IsEqual(Column.Parameter(erpId)) as Select).ExecuteScalar<Guid>();
		}

		private string GetInsertHead()
		{
			return "Insert into Product(Name, SmrERPId, Id, Code, SmrSourceId, SmrLastIntegrationDate, IsArchive, SmrDirectionId, CategoryId, TypeId, SmrGroupId, SmrRecommendedRetailPrice, SmrProvider, SmrSizeId, TradeMarkId) VALUES";
		}

		public string GetInsertValues(ProductGateInfo info, Guid? directionId, Guid? categoryId, Guid? subCategoryId, Guid? groupId, Guid? sizeId, Guid? tradeMarkId)
		{
			return String.Format("('{0}', '{1}', '{2}', '{3}', '{4}', GETUTCDATE(), {5}, {6}, {7}, {8}, {9}, {10}, '{11}', {12}, {13})", info.Name, info.ERPId, info.Id, info.Code, ERPDataSourceId, info.IsArchived ? "1" : "0",
				directionId == Guid.Empty ? "null" : String.Format("'{0}'", directionId),
				categoryId == Guid.Empty ? "null" : String.Format("'{0}'", categoryId),
				subCategoryId == Guid.Empty ? "null" : String.Format("'{0}'", subCategoryId),
				groupId == Guid.Empty ? "null" : String.Format("'{0}'", groupId),
				info.RecommendedRetailPrice.ToString().Replace(",", "."),
				info.Provider,
				sizeId == Guid.Empty ? "null" : String.Format("'{0}'", sizeId),
				tradeMarkId == Guid.Empty ? "null" : String.Format("'{0}'", tradeMarkId)
				);
		}

		private Insert GetInsertQuery(ProductGateInfo info, Guid? directionId, Guid? categoryId, Guid? subCategoryId, Guid? groupId, Guid? sizeId, Guid? tradeMarkId)
		{
			return new Insert(UserConnection)
						.Into(_tableName)
						.Set("Name", Column.Parameter(info.Name))
						.Set("Code", Column.Parameter(info.Code))
						.Set("SmrSourceId", Column.Parameter(ERPDataSourceId))
						.Set("SmrLastIntegrationDate", Column.Parameter(DateTime.UtcNow))
						.Set("IsArchive", Column.Parameter(info.IsArchived))
						.Set("SmrDirectionId", Column.Parameter(directionId))
						.Set("CategoryId", Column.Parameter(categoryId))
						.Set("TypeId", Column.Parameter(subCategoryId))
						.Set("SmrGroupId", Column.Parameter(groupId))
						.Set("SmrRecommendedRetailPrice", Column.Parameter(info.RecommendedRetailPrice))
						.Set("SmrProvider", Column.Parameter(info.Provider))
						.Set("SmrSizeId", Column.Parameter(sizeId))
						.Set("TradeMarkId", Column.Parameter(tradeMarkId))
						.Set("Id", Column.Parameter(info.Id))
						.Set("SmrERPId", Column.Parameter(info.ERPId)) as Insert;
		}

		private Update GetUpdateQuery(ProductGateInfo info, Guid? directionId, Guid? categoryId, Guid? subCategoryId, Guid? groupId, Guid? sizeId, Guid? tradeMarkId)
		{
			var update = new Update(UserConnection, _tableName)
						.Set("Name", Column.Parameter(info.Name))
						.Set("Code", Column.Parameter(info.Code))
						.Set("SmrSourceId", Column.Parameter(ERPDataSourceId))
						.Set("SmrLastIntegrationDate", Column.Parameter(DateTime.UtcNow))
						.Set("IsArchive", Column.Parameter(info.IsArchived))
						.Set("SmrDirectionId", Column.Parameter(directionId))
						.Set("CategoryId", Column.Parameter(categoryId))
						.Set("TypeId", Column.Parameter(subCategoryId))
						.Set("SmrGroupId", Column.Parameter(groupId))
						.Set("SmrRecommendedRetailPrice", Column.Parameter(info.RecommendedRetailPrice))
						.Set("SmrProvider", Column.Parameter(info.Provider))
						.Set("TradeMarkId", Column.Parameter(tradeMarkId))
						.Set("SmrSizeId", Column.Parameter(sizeId))
						as Update;

			update.Where("SmrERPId").IsEqual(Column.Parameter(info.ERPId));

			return update;
		}
	}
}
