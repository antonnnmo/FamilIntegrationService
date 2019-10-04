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

            sb.AppendLine(String.Join(",", shops.Select(c => String.Format(@"('{0}', '{1}', '{2}')", c.Name, c.Code, c.Id))));
            return sb.ToString();
        }

        protected override string GetQuery(BaseProcessingModel model)
        {
            var shop = (ShopProcessingModel)model;
            return string.Format(@"INSERT INTO ""public"".""Shop"" (""Name"", ""Code"", ""Id"") VALUES ('{0}', '{1}', '{2}')", shop.Name, shop.Code, shop.Id);
        }
    }
}
