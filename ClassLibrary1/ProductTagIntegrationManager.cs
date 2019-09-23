using FamilIntegrationService.Models;
using System;
using System.Collections.Generic;
using System.Text;
using Terrasoft.Core;
using Terrasoft.Core.DB;
using Newtonsoft.Json;

namespace Terrasoft.Configuration
{
	public class ProductInPurchaseIntegrationManager
	{
		private static readonly string ERPDataSourceId = "27F9DE70-4B0F-423C-AC62-C4730414F3B3";
		private LookupManager _lookupManager;
		private string _tableName = "SmrProductInPurchase";

		public UserConnection UserConnection { get; private set; }

		public ProductInPurchaseIntegrationManager(UserConnection uc)
		{
			UserConnection = uc;
			_lookupManager = new LookupManager(uc);
		}

		public PackResult PrimaryIntegratePack(string request)
		{
			var objs = JsonConvert.DeserializeObject<List<ProductInPurchaseGateInfo>>(request);
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

		private Guid GetObjectId(string erpId)
		{
			return (new Select(UserConnection).Top(1).Column("Id").From(_tableName).Where("SmrERPId").IsEqual(Column.Parameter(erpId)) as Select).ExecuteScalar<Guid>();
		}

		private string GetInsertHead()
		{
			return "Insert into SmrProductInPurchase(SmrPurchaseId, ProductId, Price, Quantity, Amount) VALUES";
		}

		public string GetInsertValues(ProductInPurchaseGateInfo info)
		{
			return $"((Select Id from Purchase WITH(NOLOCK) Where SmrERPId = '{info.PurchaseId}'), (Select Id from Product WITH(NOLOCK) Where Code = '{info.ProductCode}'), {info.Price.ToString().Replace(",", ".")}, {info.Quantity}, {info.Amount.ToString().Replace(",", ".")})";
		}
	}
}
