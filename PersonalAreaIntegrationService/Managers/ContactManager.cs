using FamilIntegrationCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessingIntegrationService.Managers
{
	public class ContactManager : BaseManager
	{
		private readonly Guid _defaultBrandId = new Guid("fa2e8409-c20e-464b-a034-16024a39a3d7");

		protected override string GetPrimaryQuery(IEnumerable<BaseProcessingModel> models)
		{
			var contacts = models.Select(m => (ContactProcessingModel)m);
			var sb = new StringBuilder();
			sb.AppendLine(@"INSERT INTO ""public"".""Contact"" (""Name"", ""Phone"", ""Id"", ""BrandId"", 
						""Name_Surname"", 
						""Name_MiddleName"", 
						""Email"",
						""Gender"",
						""Address_Country"",
						""Address_City"",
						""Address_Address"",
						""Birthday"",
						""CustomFields"",
						""RegistrationPlaceId"",
						""RegistrationDate"",
						""PhoneConfirmed"") VALUES ");

			sb.AppendLine(String.Join(",", contacts.Select(c => String.Format(@"('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}', '{9}', '{10}', {11}, '{{ ""SmrRequiresCorrection"": ""{12}"", ""SmrThereAreEmptyFields"": ""{13}"", ""SmrPersDataProcAgreement"": ""{14}""}}', (SELECT ""Id"" FROM ""public"".""Shop"" WHERE ""Code"" = '{15}'), {16}, true)",
				(c.FirstName ?? String.Empty).Replace("'", "''"),
				(c.Phone ?? String.Empty).Replace("'", "''"),
				c.Id.ToString(),
				_defaultBrandId,
				(c.Surname ?? String.Empty).Replace("'", "''"),
				(c.MiddleName ?? String.Empty).Replace("'", "''"),
				(c.Email ?? String.Empty).Replace("'", "''"),
				(c.Gender ?? String.Empty).Replace("'", "''"),
				(c.Country ?? String.Empty).Replace("'", "''"),
				(c.City ?? String.Empty).Replace("'", "''"),
				(c.Address ?? String.Empty).Replace("'", "''"),
				String.IsNullOrEmpty(c.Birthday) ? "null" : String.Format("'{0}'", c.Birthday.Replace("'", "''")),
				(c.RequiresCorrection ?? String.Empty).Replace("'", "''"),
				(c.ThereAreEmptyFields ?? String.Empty).Replace("'", "''"),
				(c.PersDataProcAgreement ?? String.Empty).Replace("'", "''"),
				(c.ShopCode ?? String.Empty).Replace("'", "''"),
				String.IsNullOrEmpty(c.RegistrationDate) ? "null" : String.Format("'{0}'", c.RegistrationDate.Replace("'", "''"))
				))));
			return sb.ToString();
		}

		protected override string GetQuery(BaseProcessingModel model)
		{
			var contact = (ContactProcessingModel)model;
			return string.Format
				(@"
                do $$ begin
                if (select 1 from ""Contact"" where ""Id""='{2}') then
                    UPDATE 
						""public"".""Contact"" 
					SET 
						""Name_FirstName"" = '{0}', 
						""Phone"" = '{1}', 
						""BrandId"" = '{3}', 
						""Name_Surname"" = '{4}', 
						""Name_MiddleName"" = '{5}',
						""Email"" = '{6}',
						""Gender"" = '{7}',
						""Address_Country"" = '{8}',
						""Address_City"" = '{9}',
						""Address_Address"" = '{10}',
						""Birthday"" = {11},
						""CustomFields"" = '{{ ""SmrRequiresCorrection"": ""{12}"", ""SmrThereAreEmptyFields"": ""{13}"", ""SmrPersDataProcAgreement"": ""{14}""}}',
						""RegistrationPlaceId"" = (SELECT ""Id"" FROM ""public"".""Shop"" WHERE ""Code"" = '{15}'),
						""RegistrationDate"" = {16},
						""PhoneConfirmed"" = true
					WHERE ""Id"" = '{2}';
                ELSE
                    INSERT INTO ""public"".""Contact"" (
						""Name_FirstName"", 
						""Phone"", 
						""Id"", 
						""BrandId"", 
						""Name_Surname"", 
						""Name_MiddleName"", 
						""Email"",
						""Gender"",
						""Address_Country"",
						""Address_City"",
						""Address_Address"",
						""Birthday"",
						""CustomFields"",
						""RegistrationPlaceId"",
						""RegistrationDate"",
						""PhoneConfirmed""

						) VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}', '{9}', '{10}', {11}, '{{ ""SmrRequiresCorrection"": ""{12}"", ""SmrThereAreEmptyFields"": ""{13}"", ""SmrPersDataProcAgreement"": ""{14}""}}', (SELECT ""Id"" FROM ""public"".""Shop"" WHERE ""Code"" = '{15}'), {16}, true);
                END IF;
                END $$
                ",
				(contact.FirstName ?? String.Empty).Replace("'", "''"),
				(contact.Phone ?? String.Empty).Replace("'", "''"),
				contact.Id.ToString(),
				_defaultBrandId,
				(contact.Surname ?? String.Empty).Replace("'", "''"),
				(contact.MiddleName ?? String.Empty).Replace("'", "''"),
				(contact.Email ?? String.Empty).Replace("'", "''"),
				(contact.Gender ?? String.Empty).Replace("'", "''"),
				(contact.Country ?? String.Empty).Replace("'", "''"),
				(contact.City ?? String.Empty).Replace("'", "''"),
				(contact.Address ?? String.Empty).Replace("'", "''"),
				String.IsNullOrEmpty(contact.Birthday) ? "null" : String.Format("'{0}'", contact.Birthday.Replace("'", "''")),
				(contact.RequiresCorrection ?? String.Empty).Replace("'", "''"),
				(contact.ThereAreEmptyFields ?? String.Empty).Replace("'", "''"),
				(contact.PersDataProcAgreement ?? String.Empty).Replace("'", "''"),
				(contact.ShopCode ?? String.Empty).Replace("'", "''"),
				String.IsNullOrEmpty(contact.RegistrationDate) ? "null" : String.Format("'{0}'", contact.RegistrationDate.Replace("'", "''"))


				);
		}
	}
}
