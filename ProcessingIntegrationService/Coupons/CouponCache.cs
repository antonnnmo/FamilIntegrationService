using FamilIntegrationService;
using FamilIntegrationService.Providers;
using Npgsql;
using ProcessingIntegrationService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProcessingIntegrationService.Coupons
{
	public class CouponCache
	{
		

		static List<Coupon> _coupons;
		public static List<Coupon> Coupons
		{
			get
			{
				if (_coupons == null)
				{
					LoadFromDB();
				}
				return _coupons;
			}
		}

		private static void LoadFromDB()
		{
			_coupons = new List<Coupon>();

			LoadCoupons();
			LoadTexts();
			LoadPromotions();
		}

		private static void SaveToDB()
		{
			using (var conn = new NpgsqlConnection(GetConnectionString()))
			{
				conn.Open();

				using (var cmd = new NpgsqlCommand(@"delete	FROM ""public"".""CouponPromotion""; delete	FROM ""public"".""CouponText""; delete	FROM ""public"".""Coupon"";", conn))
				{
					cmd.ExecuteNonQuery();
				}

				foreach (var coupon in _coupons)
				{
					var isActive = coupon.IsActive ? 1 : 0;

					using (var cmd = new NpgsqlCommand(@$"Insert into ""public"".""Coupon""(""Id"",
															""IsActive"",
															""Name"")
															VALUES(
																'{coupon.Id}',
																{isActive},
																'{coupon.Name}'
															)", conn))
					{
						cmd.ExecuteNonQuery();
					}

					if (coupon.Texts != null)
					{
						foreach (var text in coupon.Texts)
						{
							using (var cmd = new NpgsqlCommand(@$"Insert into ""public"".""CouponText""(""Id"",
																""CouponId"",
																""Text"",
																""Order"")
																VALUES(
																	'{text.Id}',
																	'{coupon.Id}',
																	'{text.Text}',
																	{text.Order}
																)", conn))
							{
								cmd.ExecuteNonQuery();
							}
						}
					}



					if (coupon.Promotions != null)
					{
						foreach (var promotion in coupon.Promotions)
						{
							using (var cmd = new NpgsqlCommand(@$"Insert into ""public"".""CouponPromotion""(""Id"",
																""CouponId"")
																VALUES(
																	'{promotion.Id}',
																	'{coupon.Id}'
																)", conn))
							{
								cmd.ExecuteNonQuery();
							}
						}
					}
				}
			}
		}

		private static void LoadPromotions()
		{
			using (var conn = new NpgsqlConnection(GetConnectionString()))
			{
				conn.Open();

				// Retrieve all rows
				using (var cmd = new NpgsqlCommand(@"SELECT ""Id"",
															""CouponId""
														FROM ""public"".""CouponPromotion""", conn))
				{
					using (var reader = cmd.ExecuteReader())
					{
						while (reader.Read())
						{
							var promotion = new PromotionDto()
							{
								Id = Read(reader, "Id", Guid.Empty)
							};

							var couponId = Read(reader, "CouponId", Guid.Empty);

							var coupon = Coupons.First(c => c.Id == couponId);
							if (coupon.Promotions == null) coupon.Promotions = new List<PromotionDto>();
							coupon.Promotions.Add(promotion);
						}
					}
				}
			}
		}

		private static void LoadTexts()
		{
			using (var conn = new NpgsqlConnection(GetConnectionString()))
			{
				conn.Open();

				// Retrieve all rows
				using (var cmd = new NpgsqlCommand(@"SELECT ""Id"",
															""CouponId"",
															""Text"",
															""Order""
														FROM ""public"".""CouponText""", conn))
				{
					using (var reader = cmd.ExecuteReader())
					{
						while (reader.Read())
						{
							var couponText = new CouponText()
							{
								Id = Read(reader, "Id", Guid.Empty),
								Order = Read(reader, "Order", 0),
								Text = Read(reader, "Text", String.Empty),
							};

							var couponId = Read(reader, "CouponId", Guid.Empty);

							var coupon = Coupons.First(c => c.Id == couponId);
							if (coupon.Texts == null) coupon.Texts = new List<CouponText>();
							coupon.Texts.Add(couponText);
						}
					}
				}
			}
		}

		private static void LoadCoupons()
		{
			using (var conn = new NpgsqlConnection(GetConnectionString()))
			{
				conn.Open();

				// Retrieve all rows
				using (var cmd = new NpgsqlCommand(@"SELECT ""Id"",
															""IsActive"",
															""Name""
														FROM ""public"".""Coupon""", conn))
				{
					using (var reader = cmd.ExecuteReader())
					{
						while (reader.Read())
						{
							_coupons.Add(new Coupon
							{
								Id = Read(reader, "Id", Guid.Empty),
								IsActive = Read(reader, "IsActive", 0) > 0,
								Name = Read(reader, "Name", String.Empty)
							});
						}
					}
				}
			}
		}

		private static string GetConnectionString()
		{
			GlobalCacheReader.GetValue(GlobalCacheReader.CacheKeys.ConnectionString, out string connString);
			return connString;
		}

		private static T Read<T>(NpgsqlDataReader reader, string columnName, T defValue)
		{
			var ordinal = reader.GetOrdinal(columnName);
			if (reader.IsDBNull(ordinal)) return defValue;

			var obj = reader.GetValue(ordinal);
			try
			{
				return (T)obj;
			}
			catch (InvalidCastException e)
			{
				throw new InvalidCastException("Invalid type for column " + columnName);
			}
			catch (Exception e)
			{
				throw new Exception("Error in column reading: " + columnName, e);
			}
		}

		public static void CreateTableIfNotExist()
		{
			var command =
			@"CREATE TABLE IF NOT EXISTS public.""Coupon""
            (
               ""Id"" uuid PRIMARY KEY,
               ""Name"" text,
               ""IsActive"" integer
            );
			CREATE TABLE IF NOT EXISTS public.""CouponText""
            (
               ""Id"" uuid PRIMARY KEY,
               ""CouponId"" uuid,
               ""Text"" text,
				""Order"" integer
            );
			CREATE TABLE IF NOT EXISTS public.""CouponPromotion""
            (
               ""Id"" uuid PRIMARY KEY,
               ""PromotionId"" uuid,
				""CouponId""  uuid
            );";
			using (var conn = new NpgsqlConnection(GetConnectionString()))
			{
				conn.Open();

				// Retrieve all rows
				using (var cmd = new NpgsqlCommand(command, conn))
				{
					cmd.ExecuteNonQuery();
				}
			}
		}

		internal static void UpdateCoupon(Coupon coupon)
		{
			if (Coupons.Any(c => c.Id == coupon.Id))
			{
				Coupons.Remove(Coupons.First(c => c.Id == coupon.Id));
			}
			Coupons.Add(coupon);

			SaveToDB();
		}
	}
}
