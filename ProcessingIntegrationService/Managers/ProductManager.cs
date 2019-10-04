using FamilIntegrationCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessingIntegrationService.Managers
{
    public class ProductManager : BaseManager
    {
        protected override string GetPrimaryQuery(IEnumerable<BaseProcessingModel> models)
        {
            var products = models.Select(m => (ProductProcessingModel)m);
            var sb = new StringBuilder();
            sb.AppendLine(@"INSERT INTO ""public"".""Product"" (""Name"", ""Code"", ""Id"") VALUES ");

            sb.AppendLine(String.Join(",", products.Select(c => String.Format(@"('{0}', '{1}', '{2}')", c.Name, c.Code, c.Id))));
            return sb.ToString();
        }

        protected override string GetQuery(BaseProcessingModel model)
        {
            var product = (ProductProcessingModel)model;
            return string.Format(@"INSERT INTO ""public"".""Product"" (""Name"", ""Code"", ""Id"") VALUES ('{0}', '{1}', '{2}')", product.Name, product.Code, product.Id);
        }
    }
}
