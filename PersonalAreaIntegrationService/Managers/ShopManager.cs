using FamilIntegrationCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessingIntegrationService.Managers
{
    public class ShopManager : BaseManager
    {
        protected override string GetPrimaryQuery(IEnumerable<BaseProcessingModel> models)
        {
            var shops = models.Select(m => (ShopProcessingModel)m);
            var sb = new StringBuilder();
            sb.AppendLine(@"INSERT INTO ""public"".""Shop"" (""Name"", ""Code"", ""Id"") VALUES ");

            sb.AppendLine(String.Join(",", shops.Select(c => String.Format(@"('{0}', '{1}', '{2}')", (c.Name ?? "").Replace("'", "''"), (c.Code ?? "").Replace("'", "''"), c.Id))));
            return sb.ToString();
        }

        protected override string GetQuery(BaseProcessingModel model)
        {
            var shop = (ShopProcessingModel)model;
            return string.Format(
                @"
                do $$ begin
                if (select 1 from ""Shop"" where ""Id""='{2}') then
                    UPDATE ""public"".""Shop"" SET ""Name"" = '{0}', ""Code"" = '{1}' WHERE ""Id"" = '{2}';
                ELSE
                    INSERT INTO ""public"".""Shop"" (""Name"", ""Code"", ""Id"") VALUES ('{0}', '{1}', '{2}');
                END IF;
                END $$
                ", (shop.Name ?? "").Replace("'", "''"), (shop.Code ?? "").Replace("'", "''"), shop.Id);
        }
    }
}
