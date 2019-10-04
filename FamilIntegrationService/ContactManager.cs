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
	public class ContactManager: BaseManager
	{
		private static readonly string _selectContact = @"SELECT TOP ({0}) [gateId]
      ,[ERPId]
      ,[source]
      ,[status]
      ,[errorMessage]
      ,CONVERT(nvarchar(50), createdOn, 21) as CreatedOn
      ,[firstName]
      ,[surname]
      ,[middleName]
      ,[email]
      ,[phone]
      ,CONVERT(nvarchar(50), birthday, 21) as birthday
      ,[gender]
      ,[country]
      ,[city]
      ,[address]
      ,[SmrNearestMetroStation]
      ,CONVERT(nvarchar(50), registrationDate, 21) as RegistrationDate
      ,[shopCode]
      ,[contactStatus]
      ,[requiresCorrection]
      ,[persDataProcAgreement]
      ,[thereAreEmptyFields]
      ,[bonusBalance]
  FROM [ContactGate]
 Where Status = 0 And Source = 0";

		protected List<Contact> _contacts;

		public ContactManager()
		{
			_tableName = "ContactGate";
			_processingPrimaryMethodName = "LoadPrimaryContactPack";
			_processingMethodName = "LoadContactPack";
			_isNeedSendToProcessing = true;
		}

		protected override List<BaseIntegrationObject> ReadPack() {
			var pack = new List<BaseIntegrationObject>();
			lock (_lock)
			{
				using (var provider = new DBConnectionProvider())
				{
					using (var reader = provider.Execute(_selectContact, packSize))
					{
						while (reader != null && reader.Read())
						{
							pack.Add(new Contact()
							{
								Address = reader.GetValue("address", String.Empty),
								City = reader.GetValue("City", String.Empty),
								Country = reader.GetValue("Country", String.Empty),
								ContactStatus = reader.GetValue("contactStatus", String.Empty),
								Email = reader.GetValue("email", String.Empty),
								ErrorMessage = reader.GetValue("errorMessage", String.Empty),
								FirstName = reader.GetValue("FirstName", String.Empty),
								MiddleName = reader.GetValue("MiddleName", String.Empty),
								Phone = reader.GetValue("Phone", String.Empty),
								RegistrationDate = reader.GetValue("RegistrationDate", String.Empty),
								CreatedOn = reader.GetValue("CreatedOn", String.Empty),
								BirthDay = reader.GetValue("birthday", String.Empty),
								ERPId = reader.GetValue("ERPId", String.Empty),
								Id = Guid.NewGuid(),
								Surname = reader.GetValue("Surname", String.Empty),
								ShopCode = reader.GetValue("ShopCode", String.Empty),
								SmrNearestMetroStation = reader.GetValue("SmrNearestMetroStation", String.Empty),
								BonusBalance = reader.GetValue("bonusBalance", 0m),
								IsMan = reader.GetValue("gender", false),
								PersDataProcAgreement = reader.GetValue("PersDataProcAgreement", false),
								RequiresCorrection = reader.GetValue("RequiresCorrection", false),
								ThereAreEmptyFields = reader.GetValue("ThereAreEmptyFields", String.Empty)
							});
						}
					}
				}

                if (pack.Count > 0)
                    DBConnectionProvider.ExecuteNonQuery(String.Format("Update ContactGate Set Status = 3 Where ERPId in ({0})", String.Join(",", pack.Select(p => String.Format("'{0}'", p.ERPId)))));
			}

			return pack;
		}

		protected override string GetSerializedCollection(List<BaseIntegrationObject> pack)
		{
			return JsonConvert.SerializeObject(pack.Select(p => (Contact)p).ToList());
		}

		protected override string GetProcessingPackBody(List<BaseIntegrationObject> pack)
		{
			var contacts = pack.Select(c => (Contact)c).Select(c => new ContactProcessingModel() { ERPId = c.ERPId, Id = GetCustomFieldsGuidValue(c, "ContactId") ?? c.Id, Name = GetContactName(c.Surname, c.FirstName, c.MiddleName), Phone = c.Phone });
			return JsonConvert.SerializeObject(contacts);
		}

        private string GetContactName(string surname, string firstName, string middleName)
        {
            var names = new List<string> { surname, firstName, middleName };
            return string.Join(" ", names);
        }
    }
}
