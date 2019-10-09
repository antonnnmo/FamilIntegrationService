using FamilIntegrationService.Providers;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FamilIntegrationService
{
	public static class AnswerTemplateCollection
	{
		static List<AnswerTemplate> _templates;
		public static List<AnswerTemplate> Templates
		{
			get
			{
				if (_templates == null)
				{
					LoadFromDB();
				}
				return _templates;
			}
		}

		public static void SaveToDB(List<AnswerTemplate> templates)
		{
            CreateTableIfNotExist();
            using (var conn = new NpgsqlConnection(GetConnectionString()))
			{
				conn.Open();
				using (var cmd = new NpgsqlCommand(@"delete from ""public"".""AnswerTemplate""", conn))
				{
					cmd.ExecuteNonQuery();
				}
				foreach (var template in templates)
				{
					using (var cmd = new NpgsqlCommand(@"INSERT INTO ""public"".""AnswerTemplate"" (""PrefixText"", ""SuffixText"", ""From"", ""To"", ""Price"", ""Start"", ""End"", ""IsFirstTextBlock"", ""Id"") VALUES (@p1, @p2, @p3, @p4, @p5, @p6, @p7, @p8, @p9)", conn))
					{
						cmd.Parameters.AddWithValue("p1", template.PrefixText);
						cmd.Parameters.AddWithValue("p2", template.SuffixText);
						cmd.Parameters.AddWithValue("p3", template.From);
						cmd.Parameters.AddWithValue("p4", template.To);
						cmd.Parameters.AddWithValue("p5", template.Price);
						cmd.Parameters.AddWithValue("p6", template.Start);
						cmd.Parameters.AddWithValue("p7", template.End);
						cmd.Parameters.AddWithValue("p8", template.IsFirstTextBlock ? 1: 0);
                        cmd.Parameters.AddWithValue("p9", Guid.NewGuid());
                        cmd.ExecuteNonQuery();
					}
				}
			}
			_templates = templates;

		}

        private static void CreateTableIfNotExist()
        {
            var command =
            @"CREATE TABLE IF NOT EXISTS public.""AnswerTemplate""
            (
               ""Id"" uuid PRIMARY KEY,
               ""PrefixText"" text,
               ""SuffixText"" text,
               ""From"" decimal,
               ""To"" decimal,
               ""Price"" decimal,
               ""Start"" timestamp without time zone,
               ""End"" timestamp without time zone,
               ""IsFirstTextBlock"" integer
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

		private static string GetConnectionString() {
			GlobalCacheReader.GetValue(GlobalCacheReader.CacheKeys.ConnectionString, out string connString);
			return connString;
		}

		private static void LoadFromDB()
		{
            CreateTableIfNotExist();
            _templates = new List<AnswerTemplate>();
			
			using (var conn = new NpgsqlConnection(GetConnectionString()))
			{
				conn.Open();

				// Retrieve all rows
				using (var cmd = new NpgsqlCommand(@"SELECT ""PrefixText"",
															""SuffixText"",
															""From"",
															""To"",
															""Price"",
															""Start"",
															""End"",
															""IsFirstTextBlock""
														FROM ""public"".""AnswerTemplate""", conn))
				{
					using (var reader = cmd.ExecuteReader())
					{
						while (reader.Read())
						{
							_templates.Add(new AnswerTemplate {
								End = Read(reader, "End", DateTime.MaxValue),
								Start = Read(reader, "Start", DateTime.MinValue),
								From = Read(reader, "From", 0m),
								To = Read(reader, "To", 0m),
								Price = Read(reader, "Price", 0m),
								IsFirstTextBlock = Read(reader, "IsFirstTextBlock", 0) == 1,
								PrefixText = Read(reader, "PrefixText", String.Empty),
								SuffixText = Read(reader, "SuffixText", String.Empty)
							});
						}
					}
				}
			}
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
	}
}
