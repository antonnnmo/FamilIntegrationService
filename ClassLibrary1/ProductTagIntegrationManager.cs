using FamilIntegrationService.Models;
using System;
using System.Collections.Generic;
using System.Text;
using Terrasoft.Core;
using Terrasoft.Core.DB;
using Newtonsoft.Json;

namespace Terrasoft.Configuration
{
	public class PurchaseIntegrationManager
	{
		private static readonly string ERPDataSourceId = "27F9DE70-4B0F-423C-AC62-C4730414F3B3";
		private LookupManager _lookupManager;
		private string _tableName = "SmrPurchase";

		public UserConnection UserConnection { get; private set; }

		public PurchaseIntegrationManager(UserConnection uc)
		{
			UserConnection = uc;
			_lookupManager = new LookupManager(uc);
		}

		private Guid GetContactId(string contactId)
		{
			return (new Select(UserConnection).Column("Id").From("Contact").Where("SmrERPId").IsEqual(Column.Parameter(contactId)) as Select).ExecuteScalar<Guid>();
		}

		public PackResult PrimaryIntegratePack(string request)
		{
			var objs = JsonConvert.DeserializeObject<List<PurchaseGateInfo>>(request);
			var lookupManager = new LookupManager(UserConnection);
			var result = new List<PackResult>();
			var sb = new StringBuilder();
			sb.AppendLine(GetInsertHead());
			var values = new List<string>();
			foreach (var info in objs)
			{
				var shopId = lookupManager.FindLookupIdByCode(info.ShopCode, "SmrShop");
				var cashDeskId = lookupManager.FindLookupIdByCode(info.CashDeskCode, "SmrCashdesk");
				var paymentFormId = lookupManager.FindLookupIdByName(info.PaymentForm, "SmrPaymentForm");

				values.Add(GetInsertValues(info, shopId, cashDeskId, paymentFormId));
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
			return "Insert into SmrPurchase(Number, SmrERPId, Id, ContactId, SmrLastIntegrationDate, ShopId, CashdeskId, PaymentFormId, Total, SmrSourceId, Date) VALUES";
		}

		public string GetInsertValues(PurchaseGateInfo info, Guid? shopId, Guid? cashDeskId, Guid? paymentFormId)
		{
			var shop = shopId == Guid.Empty ? "null" : String.Format("'{0}'", shopId);
			var cashDesk = cashDeskId == Guid.Empty ? "null" : String.Format("'{0}'", cashDeskId);
			var paymentForm = paymentFormId == Guid.Empty ? "null" : String.Format("'{0}'", paymentFormId);
			return $"('{info.Number}', '{info.ERPId}', '{info.Id}', (Select Id from Contact WITH(NOLOCK) Where SmrERPId = '{info.ContactId}'), GETUTCDATE(), {shop}, {cashDesk}, {paymentForm}, {info.Amount.ToString().Replace(",", ".")}, '{ERPDataSourceId}', CONVERT(datetime, '{info.PurchaseDate}', 21))";
		}
	}
}
