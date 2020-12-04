using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FamilIntegrationService.Providers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using ProcessingIntegrationService.Models;

namespace ProcessingIntegrationService.Controllers
{
	[Route("card")]
	[ApiController]
	public class CardController : ControllerBase
	{
		private static object _lock = new object();
		[HttpPost("generate")]
		public ActionResult GenerateCard() 
		{
			var stringNumber = String.Empty;
			//var lastUsedNumber = 0;
			var max_try_count = 10000;

			do
			{
				var newNumber = GetNewNumber();

				if (newNumber > 9999999) return Ok(new CardGenerateResponse() { Success = false, Error = "Карты закончились" });

				stringNumber = CalculateEan13($"99115{newNumber.ToString().PadLeft(7, '0')}");

				max_try_count--;
			} while (max_try_count > 0 && IsCardExist(stringNumber));

			if (max_try_count <= 0) 
			{
				return Ok(new CardGenerateResponse() { Success = false, Error = "Не удалось сгенерировать карту за 10000 попыток." });
			}

			SaveCard(stringNumber);

			return Ok(new CardGenerateResponse() { Success = true, Data = new CardGenerateData() { Number = stringNumber } });
		}

		private void SaveCard(string number)
		{
			var cardId = Guid.NewGuid();
			using (var conn = new NpgsqlConnection(GetConnectionString()))
			{
				conn.Open();
				using (var cmd = new NpgsqlCommand($@"Insert INTO ""public"".""Card""(""Id"", ""Number"", ""State"") VALUES('{cardId}', '{number}', 0);
												Insert INTO ""public"".""CardTemp""(""CardId"", ""Number"", ""IsSendedToCS"", ""IsSendedToBPM"", ""AttemptCount"", ""LastError"") VALUES('{cardId}', '{number}', 0, 0, 0, '');", conn))
				{
					cmd.ExecuteScalar();
				}
			}
		}

		private bool IsCardExist(string stringNumber)
		{
			using (var conn = new NpgsqlConnection(GetConnectionString()))
			{
				conn.Open();

				// Retrieve all rows
				using (var cmd = new NpgsqlCommand($@"SELECT ""Id"" FROM ""public"".""Card"" Where ""Number"" = '{stringNumber}'", conn))
				{
					return cmd.ExecuteScalar() != null;
				}
			}
		}

		private int GetNewNumber()
		{
			var newNumber = 0;
			lock (_lock)
			{
				
				var res = String.Empty;
				using (var conn = new NpgsqlConnection(GetConnectionString()))
				{
					conn.Open();

					// Retrieve all rows
					using (var cmd = new NpgsqlCommand(@"SELECT ""Value"" FROM ""public"".""Settings"" Where ""Code"" = 'LastCardNumber'", conn))
					{
						res = cmd.ExecuteScalar()?.ToString() ?? "0";
						newNumber = Convert.ToInt32(res) + 1;
					}

					if (res == "0")
					{
						using (var cmd = new NpgsqlCommand(@"INSERT INTO ""public"".""Settings""(""Code"", ""Value"") VALUES('LastCardNumber', '1')", conn)) { cmd.ExecuteNonQuery(); }
					}
					else 
					{
						using (var cmd = new NpgsqlCommand($@"Update ""public"".""Settings"" SET ""Value"" = '{newNumber}' Where ""Code"" = 'LastCardNumber'", conn)) { cmd.ExecuteNonQuery(); }
					}
				}
			}

			return newNumber;
		}

		private static string GetConnectionString()
		{
			GlobalCacheReader.GetValue(GlobalCacheReader.CacheKeys.ConnectionString, out string connString);
			return connString;
		}

		public static string CalculateEan13(string number)
		{
			int sum = 0;
			int digit = 0;

			// Calculate the checksum digit here.
			for (int i = number.Length; i >= 1; i--)
			{
				digit = Convert.ToInt32(number.Substring(i - 1, 1));
				// This appears to be backwards but the 
				// EAN-13 checksum must be calculated
				// this way to be compatible with UPC-A.
				if (i % 2 == 0)
				{ // odd  
					sum += digit * 3;
				}
				else
				{ // even
					sum += digit * 1;
				}
			}
			int checkSum = (10 - (sum % 10)) % 10;
			return $"{number}{checkSum}";
		}

		public static void CreateTableIfNotExist()
		{
			var command =
			@"CREATE TABLE IF NOT EXISTS public.""CardTemp""
            (
				""CardId"" uuid PRIMARY KEY,
				""Number"" text,
				""IsSendedToCS"" integer,
				""IsSendedToBPM"" integer,
				""AttemptCount"" integer, 
				""LastError"" text
            );
			";
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
	}
}
