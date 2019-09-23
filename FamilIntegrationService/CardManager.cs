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
	public class CardManager : BaseManager
	{
		private static readonly string _selectContact = @"SELECT TOP ({0}) [gateId]
      ,[ERPId]
      ,[source]
      ,[status]
      ,[errorMessage]
      ,CONVERT(nvarchar(50), createdOn, 21) as CreatedOn
      ,number
	  ,contactId
	  ,cardStatus
	  ,CONVERT(nvarchar(50), activationDate, 21) as activationDate
	  ,blockingReason
	  ,CONVERT(nvarchar(50), blockedOn, 21) as blockedOn
	  ,isMain
  FROM [CardGate]
 Where Status = 0 And Source = 0";

		protected List<Contact> _contacts;

		public CardManager()
		{
			_tableName = "CardGate";
			_processingPrimaryMethodName = "LoadPrimaryCardPack";
			_processingMethodName = "LoadCardPack";
			_isNeedSendToProcessing = true;
		}

		protected override List<BaseIntegrationObject> ReadPack()
		{
			var pack = new List<BaseIntegrationObject>();
			lock (_lock)
			{
				using (var provider = new DBConnectionProvider())
				{
					using (var reader = provider.Execute(_selectContact, packSize))
					{
						while (reader != null && reader.Read())
						{
							pack.Add(new Card()
							{
								ActivationDate = reader.GetValue("activationDate", String.Empty),
								BlockedOn = reader.GetValue("blockedOn", String.Empty),
								BlockingReason = reader.GetValue("blockingReason", String.Empty),
								CardStatus = reader.GetValue("cardStatus", String.Empty),
								ContactId = reader.GetValue("contactId", String.Empty),
								ErrorMessage = reader.GetValue("errorMessage", String.Empty),
								IsMain = reader.GetValue("isMain", false),
								Number = reader.GetValue("number", String.Empty),
								CreatedOn = reader.GetValue("CreatedOn", String.Empty),
								ERPId = reader.GetValue("ERPId", String.Empty),
								Id = Guid.NewGuid()
							});
						}
					}
				}

				DBConnectionProvider.ExecuteNonQuery(String.Format("Update {1} Set Status = 3 Where ERPId in ({0})", String.Join(",", pack.Select(p => String.Format("'{0}'", p.ERPId))), _tableName));
			}

			return pack;
		}

		protected override string GetSerializedCollection(List<BaseIntegrationObject> pack)
		{
			return JsonConvert.SerializeObject(pack.Select(p => (Card)p).ToList());
		}

		protected override string GetProcessingPackBody(List<BaseIntegrationObject> pack)
		{
			var contacts = pack.Select(c => (Card)c).Select(c => new CardProcessingModel() { ERPId = c.ERPId, Id = c.Id, Number = c.Number });
			return JsonConvert.SerializeObject(contacts);
		}
	}
}
