using FamilIntegrationCore.Models;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProcessingIntegrationService.Managers
{
	public class Promocode
	{
		public static void CreateTableIfNotExists()
		{
			using (var conn = new NpgsqlConnection(DBProvider.GetConnectionString()))
			{
				conn.Open();
				var query =
					@"CREATE TABLE IF NOT EXISTS public.""PromoCodePoolInfo"" (
						""Id"" uuid NOT NULL,
						""Name"" text NULL,
						""Description"" text NULL,
					CONSTRAINT ""PK_PromoCodePoolInfo"" PRIMARY KEY(""Id"")
                    );
                    ";
				new NpgsqlCommand(query, conn).ExecuteNonQuery();
			}
		}

		public static void ChangePool(SendPromocodePoolRequest request)
		{
			using (var conn = new NpgsqlConnection(DBProvider.GetConnectionString()))
			{
				conn.Open();
				var query = string.Format(
				@"   
                do $$ begin
				if (select 1 from ""PromoCodePoolInfo"" where ""Id""='{0}') then
                    UPDATE ""public"".""PromoCodePoolInfo"" SET ""Name"" = '{1}', ""Description"" = '{2}' WHERE ""Id"" = '{0}';
                ELSE
                    INSERT INTO ""public"".""PromoCodePoolInfo"" (""Name"", ""Id"", ""Description"") VALUES ('{1}', '{0}', '{2}');
                END IF;
                END $$",
				request.Id.ToString(), request.Name.Replace("'", "''"), request.Description.Replace("'", "''"));
				new NpgsqlCommand(query, conn).ExecuteNonQuery();
			}
		}

		internal static List<ActivePromocode> GetActivePromocodes(string mobilePhone, string cardNumber)
		{
			var promo = new List<ActivePromocode>();
			using (var conn = new NpgsqlConnection(DBProvider.GetConnectionString()))
			{
				conn.Open();
				var query = String.Format(@"SELECT pc.""Code"", pi.""Name"", pi.""Description"" FROM public.""PromoCode"" pc 
								JOIN public.""PromoCodePool"" pool ON pool.""Id"" = pc.""PoolId"" AND(pc.""IsUsed"" = false OR pool.""CanUseManyTimes"" = true)
								LEFT JOIN public.""PromoCodePoolInfo"" pi ON pi.""Id"" = pool.""Id""
								WHERE pc.""ContactId"" in (SELECT ""Id"" FROM PUBLIC.""Contact"" WHERE ""Phone"" = '{0}' and ""Phone"" <> ''
								UNION
								SELECT ""ContactId"" FROM PUBLIC.""Card"" WHERE ""Number"" = '{1}' and ""Number"" <> '')", mobilePhone?.Replace("'", "''"), cardNumber?.Replace("'", "''"));
				using (var command = new NpgsqlCommand(query, conn))
				{
					using (var reader = command.ExecuteReader())
					{
						while (reader.Read())
						{
							promo.Add(new ActivePromocode() {
								Description = reader.IsDBNull(2) ? String.Empty : reader.GetString(2),
								Name = reader.IsDBNull(0) ? String.Empty : reader.GetString(0),
								Pool = reader.IsDBNull(1) ? String.Empty : reader.GetString(1),
							});
						}
					}
				}
			}

			return promo;
		}

		internal static List<ActivePromocode> GetActivePromocodes(string mobilePhone)
		{
			var promo = new List<ActivePromocode>();
			using (var conn = new NpgsqlConnection(DBProvider.GetConnectionString()))
			{
				conn.Open();
				var query = String.Format(@"SELECT pc.""Code"", pi.""Name"", pi.""Description"" FROM public.""PromoCode"" pc 
								JOIN public.""PromoCodePool"" pool ON pool.""Id"" = pc.""PoolId"" AND(pc.""IsUsed"" = false OR pool.""CanUseManyTimes"" = true)
								LEFT JOIN public.""PromoCodePoolInfo"" pi ON pi.""Id"" = pool.""Id""
								WHERE pc.""ContactId"" in (SELECT ""Id"" FROM PUBLIC.""Contact"" WHERE ""Phone"" = '{0}' and ""Phone"" <> ''
								)", mobilePhone?.Replace("'", "''"));
				using (var command = new NpgsqlCommand(query, conn))
				{
					using (var reader = command.ExecuteReader())
					{
						while (reader.Read())
						{
							promo.Add(new ActivePromocode()
							{
								Description = reader.IsDBNull(2) ? String.Empty : reader.GetString(2),
								Name = reader.IsDBNull(0) ? String.Empty : reader.GetString(0),
								Pool = reader.IsDBNull(1) ? String.Empty : reader.GetString(1),
							});
						}
					}
				}
			}

			return promo;
		}
	}
}
