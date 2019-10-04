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
        protected override string GetPrimaryQuery(IEnumerable<BaseProcessingModel> models)
        {
            var contacts = models.Select(m => (ContactProcessingModel)m);
            var sb = new StringBuilder();
            sb.AppendLine(@"INSERT INTO ""public"".""Contact"" (""Name"", ""Phone"", ""Id"") VALUES ");

            sb.AppendLine(String.Join(",", contacts.Select(c => String.Format(@"('{0}', '{1}', '{2}')", c.Name, c.Phone, c.Id))));
            return sb.ToString();
        }

        protected override string GetQuery(BaseProcessingModel model)
        {
            var contact = (ContactProcessingModel)model;
            return string.Format
                (@"
                do $$ begin
                if (select 1 from ""Contact"" where ""Id""='{2}') then
                    UPDATE ""public"".""Contact"" SET ""Name"" = '{0}', ""Phone"" = '{1}' WHERE ""Id"" = '{2}';
                ELSE
                    INSERT INTO ""public"".""Contact"" (""Name"", ""Phone"", ""Id"") VALUES ('{0}', '{1}', '{2}');
                END IF;
                END $$
                ", 
                contact.Name, contact.Phone, contact.Id.ToString());
        }
    }
}
