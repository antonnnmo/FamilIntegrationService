using FamilIntegrationCore.Models;
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
            sb.AppendLine(@"INSERT INTO ""public"".""Product"" (""Name"", ""Code"", ""Id"") VALUES ");

            sb.AppendLine(String.Join(",", products.Select(c => String.Format(@"('{0}', '{1}', '{2}')", (c.Name ?? "").Replace("'", "''"), (c.Code ?? "").Replace("'", "''"), c.Id))));

			sb.AppendLine(@"INSERT INTO ""public"".""ProductPrice"" (""Price"", ""Code"") VALUES ");

			sb.AppendLine(String.Join(",", products.Select(c => String.Format(@"({0}, '{1}')", c.Price.ToString().Replace(",", "."), (c.Code ?? "").Replace("'", "''")))));
			return sb.ToString();
        }

        protected override string GetQuery(BaseProcessingModel model)
        {
            var product = (ProductProcessingModel)model;
            return string.Format(
				@"   
                do $$ begin
                if (select 1 from ""Product"" where ""Id""='{2}') then
                    UPDATE ""public"".""Product"" SET ""Name"" = '{0}', ""Code"" = '{1}' WHERE ""Id"" = '{2}';
                ELSE
                    INSERT INTO ""public"".""Product"" (""Name"", ""Code"", ""Id"") VALUES ('{0}', '{1}', '{2}');
                END IF;
				if (select 1 from ""ProductPrice"" where ""Code""='{1}') then
                    UPDATE ""public"".""ProductPrice"" SET ""Price"" = {3} WHERE ""Code"" = '{1}';
                ELSE
                    INSERT INTO ""public"".""ProductPrice"" (""Price"", ""Code"") VALUES ('{3}', '{1}');
                END IF;
                END $$",
                (product.Name ?? "").Replace("'", "''"), (product.Code ?? "").Replace("'", "''"), product.Id, product.Price.ToString().Replace(",", "."));
        }

		internal void ChangeProductPrice(SendProductPriceRequest request)
		{
			using (var conn = new NpgsqlConnection(DBProvider.GetConnectionString()))
			{
				conn.Open();
				var query = string.Format(
				@"   
                do $$ begin
				if (select 1 from ""ProductPrice"" where ""Code""='{0}') then
                    UPDATE ""public"".""ProductPrice"" SET ""Price"" = {1} WHERE ""Code"" = '{1}';
                ELSE
                    INSERT INTO ""public"".""ProductPrice"" (""Price"", ""Code"") VALUES ('{1}', '{0}');
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
					@"CREATE TABLE IF NOT EXISTS public.""ProductPrice"" (
						""Code"" text NOT NULL,
						""Price"" double precision NULL,
					CONSTRAINT ""PK_ProductPrice"" PRIMARY KEY(""Code"")
                    );
                    ";
				new NpgsqlCommand(query, conn).ExecuteNonQuery();
			}
		}
	}
}
