using FamilIntegrationCore.Models;
using FamilIntegrationService;
using Npgsql;
using ProcessingIntegrationService.Models;
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
            sb.AppendLine(@"INSERT INTO ""public"".""Product"" (""Code"", ""Id"") VALUES ");

            sb.AppendLine(String.Join(",", products.Select(c => String.Format(@"('{0}', '{1}')", (c.Code ?? "").Replace("'", "''"), c.Id))));

			sb.Append(";");
			sb.AppendLine(@"INSERT INTO ""public"".""ProductRecommendedPrice"" (""Price"", ""Code"") VALUES ");

			sb.AppendLine(String.Join(",", products.Select(c => String.Format(@"({0}, '{1}')", c.Price.ToString().Replace(",", "."), (c.Code ?? "").Replace("'", "''")))));
			var res = sb.ToString();
			//Logger.LogInfo("запрос на добавление продукта", res);

			return res;
        }

        protected override string GetQuery(BaseProcessingModel model)
        {
            var product = (ProductProcessingModel)model;
			var productCode = (product.Code ?? "").Replace("'", "''");
			var price = product.Price.ToString().Replace(",", ".");
			return 
				$@"   
                do $$ begin
                if (select 1 from ""Product"" where ""Id""='{product.Id}') then
                    UPDATE ""public"".""Product"" SET ""Code"" = '{productCode}' WHERE ""Id"" = '{product.Id}';
                ELSE
                    INSERT INTO ""public"".""Product"" (""Code"", ""Id"") VALUES ('{productCode}', '{product.Id}');
                END IF;
				if (select 1 from ""ProductRecommendedPrice"" where ""Code""='{productCode}') then
                    UPDATE ""public"".""ProductRecommendedPrice"" SET ""Price"" = {price} WHERE ""Code"" = '{productCode}';
                ELSE
                    INSERT INTO ""public"".""ProductRecommendedPrice"" (""Price"", ""Code"") VALUES ('{price}', '{productCode}');
                END IF;
                END $$";
        }

		internal void ChangeProductRecommendedPrice(SendProductPriceRequest request)
		{
			using (var conn = new NpgsqlConnection(DBProvider.GetConnectionString()))
			{
				conn.Open();
				var query = string.Format(
				@"   
                do $$ begin
				if (select 1 from ""ProductRecommendedPrice"" where ""Code""='{0}') then
                    UPDATE ""public"".""ProductRecommendedPrice"" SET ""Price"" = {1} WHERE ""Code"" = '{0}';
                ELSE
                    INSERT INTO ""public"".""ProductRecommendedPrice"" (""Price"", ""Code"") VALUES ('{1}', '{0}');
                END IF;
                END $$",
				(request.Code ?? "").Replace("'", "''"), request.Price.ToString().Replace(",", "."));
				new NpgsqlCommand(query, conn).ExecuteNonQuery();
			}
		}

		public static void CreateTableIfNotExists()
		{
			using (var conn = new NpgsqlConnection(DBProvider.GetConnectionString()))
			{
				conn.Open();
				var query =
					@"CREATE TABLE IF NOT EXISTS public.""ProductRecommendedPrice"" (
						""Code"" text NOT NULL,
						""Price"" double precision NULL,
					CONSTRAINT ""PK_ProductRecommendedPrice"" PRIMARY KEY(""Code"")
                    );
                    ";
				new NpgsqlCommand(query, conn).ExecuteNonQuery();
			}
		}
	}
}
