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
            sb.AppendLine(@"INSERT INTO ""public"".""Contact"" (""Name"", ""Phone"", ""Id"", ""BrandId"") VALUES ");

            sb.AppendLine(String.Join(",", contacts.Select(c => String.Format(@"('{0}', '{1}', '{2}', '{3}')", (c.Name ?? "").Replace("'", "''"), (c.Phone ?? "").Replace("'", "''"), c.Id, _defaultBrandId))));
            return sb.ToString();
        }

        protected override string GetQuery(BaseProcessingModel model)
        {
            var contact = (ContactProcessingModel)model;
            return string.Format
                (@"
                do $$ begin
                if (select 1 from ""Contact"" where ""Id""='{2}') then
                    UPDATE ""public"".""Contact"" SET ""Name"" = '{0}', ""Phone"" = '{1}', ""BrandId"" = '{3}' WHERE ""Id"" = '{2}';
                ELSE
                    INSERT INTO ""public"".""Contact"" (""Name"", ""Phone"", ""Id"", ""BrandId"") VALUES ('{0}', '{1}', '{2}', '{3}');
                END IF;
                END $$
                ", 
                (contact.Name ?? "").Replace("'", "''"), (contact.Phone ?? "").Replace("'", "''"), contact.Id.ToString(), _defaultBrandId);
        }
    }
}
