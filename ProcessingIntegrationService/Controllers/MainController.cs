using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FamilIntegrationCore.Models;
using FamilIntegrationService;
using FamilIntegrationService.Models;
using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace ProcessingIntegrationService.Controllers
{
	[Route("api/Main")]
	[ApiController]
	public class MainController : ControllerBase
	{
		private static readonly string connString = "Host=stnd-prsrv-07;Username=admin;Password=password;Database=loyalty";
		// GET api/values
		[HttpGet]
		public ActionResult Get()
		{
			using (var conn = new NpgsqlConnection(connString))
			{
				conn.Open();

				// Insert some data
				using (var cmd = new NpgsqlCommand())
				{
					cmd.Connection = conn;
					cmd.CommandText = @"INSERT INTO ""public"".""Contact"" (""Name"", ""Phone"", ""Id"") VALUES (@p1, @p2, @p3)";
					cmd.Parameters.AddWithValue("p1", "bla");
					cmd.Parameters.AddWithValue("p2", "79999999999");
					cmd.Parameters.AddWithValue("p3", Guid.NewGuid());
					cmd.ExecuteNonQuery();
				}

				// Retrieve all rows
				using (var cmd = new NpgsqlCommand(@"SELECT ""Name"" FROM ""public"".""Contact""", conn))
				using (var reader = cmd.ExecuteReader())
					while (reader.Read())
						return Ok(reader.GetString(0));
			}
			return Ok("123");
		}

		[HttpPost("LoadAnswerTemplate")]
		public ActionResult LoadAnswerTemplate([FromBody]List<AnswerTemplate> templates)
		{
			if (templates != null)
			{
				AnswerTemplateCollection.SaveToDB(templates);
				return Ok(new { Result = "success" });
			}

			return BadRequest(new { Result = "parameter errors" });
		}

		[HttpPost("LoadContactPack")]
		public ActionResult LoadContactPack([FromBody]IEnumerable<ContactProcessingModel> contacts)
		{
			if (contacts == null) return BadRequest("Ошибка передачи аргументов");
			var result = new List<PackResult>();

			using (var conn = new NpgsqlConnection(connString))
			{
				conn.Open();

				foreach (var contact in contacts)
				{
					try
					{
						using (var cmd = new NpgsqlCommand())
						{
							cmd.Connection = conn;
							cmd.CommandText = String.Format(@"INSERT INTO ""public"".""Contact"" (""Name"", ""Phone"", ""Id"") VALUES ('{0}', '{1}', '{2}')", contact.Name, contact.Phone, contact.Id);
							cmd.ExecuteNonQuery();
						}

						result.Add(new PackResult() { IsSuccess = true, Id = contact.ERPId });
					}
					catch (Exception e)
					{
						result.Add(new PackResult() { IsSuccess = false, ErrorMessage = e.Message, Id = contact.ERPId });
					}
				}
			}

			return Ok(result);
		}

		[HttpPost("LoadPrimaryContactPack")]
		public ActionResult LoadPrimaryContactPack([FromBody]IEnumerable<ContactProcessingModel> contacts)
		{
			if (contacts == null) return BadRequest("Ошибка передачи аргументов");

			var queries = new List<string>();

			var sb = new StringBuilder();
			sb.AppendLine(@"INSERT INTO ""public"".""Contact"" (""Name"", ""Phone"", ""Id"") VALUES ");

			sb.AppendLine(String.Join(",", contacts.Select(c => String.Format(@"('{0}', '{1}', '{2}')", c.Name, c.Phone, c.Id))));

			using (var conn = new NpgsqlConnection(connString))
			{
				conn.Open();

				// Insert some data
				using (var cmd = new NpgsqlCommand())
				{
					cmd.Connection = conn;
					cmd.CommandText = sb.ToString();
					cmd.ExecuteNonQuery();
				}
			}

			return Ok();
		}

		[HttpPost("LoadPrimaryShopPack")]
		public ActionResult LoadPrimaryShopPack([FromBody]IEnumerable<ShopProcessingModel> contacts)
		{
			if (contacts == null) return BadRequest("Ошибка передачи аргументов");

			var queries = new List<string>();

			var sb = new StringBuilder();
			sb.AppendLine(@"INSERT INTO ""public"".""Shop"" (""Name"", ""Code"", ""Id"") VALUES ");

			sb.AppendLine(String.Join(",", contacts.Select(c => String.Format(@"('{0}', '{1}', '{2}')", c.Name, c.Code, c.Id))));

			using (var conn = new NpgsqlConnection(connString))
			{
				conn.Open();

				// Insert some data
				using (var cmd = new NpgsqlCommand())
				{
					cmd.Connection = conn;
					cmd.CommandText = sb.ToString();
					cmd.ExecuteNonQuery();
				}
			}

			return Ok();
		}

		[HttpPost("LoadShopPack")]
		public ActionResult LoadShopPack([FromBody]IEnumerable<ShopProcessingModel> contacts)
		{
			if (contacts == null) return BadRequest("Ошибка передачи аргументов");
			var result = new List<PackResult>();

			using (var conn = new NpgsqlConnection(connString))
			{
				conn.Open();

				foreach (var contact in contacts)
				{
					try
					{
						using (var cmd = new NpgsqlCommand())
						{
							cmd.Connection = conn;
							cmd.CommandText = String.Format(@"INSERT INTO ""public"".""Shop"" (""Name"", ""Code"", ""Id"") VALUES ('{0}', '{1}', '{2}')", contact.Name, contact.Code, contact.Id);
							cmd.ExecuteNonQuery();
						}

						result.Add(new PackResult() { IsSuccess = true, Id = contact.ERPId });
					}
					catch (Exception e)
					{
						result.Add(new PackResult() { IsSuccess = false, ErrorMessage = e.Message, Id = contact.ERPId });
					}
				}
			}

			return Ok(result);
		}

		[HttpPost("LoadPrimaryProductPack")]
		public ActionResult LoadPrimaryProductPack([FromBody]IEnumerable<ProductProcessingModel> contacts)
		{
			if (contacts == null) return BadRequest("Ошибка передачи аргументов");
			try
			{
				var queries = new List<string>();

				var sb = new StringBuilder();
				sb.AppendLine(@"INSERT INTO ""public"".""Product"" (""Name"", ""Code"", ""Id"") VALUES ");

				sb.AppendLine(String.Join(",", contacts.Select(c => String.Format(@"('{0}', '{1}', '{2}')", c.Name, c.Code, c.Id))));

				using (var conn = new NpgsqlConnection(connString))
				{
					conn.Open();
					 
					// Insert some data
					using (var cmd = new NpgsqlCommand())
					{
						cmd.Connection = conn;
						cmd.CommandText = sb.ToString();
						cmd.ExecuteNonQuery();
					}
				}

				return Ok();
			}
			catch (Exception e)
			{
				return BadRequest(e.Message);
			}
		}

		[HttpPost("LoadProductPack")]
		public ActionResult LoadProductPack([FromBody]IEnumerable<ProductProcessingModel> contacts)
		{
			if (contacts == null) return BadRequest("Ошибка передачи аргументов");
			var result = new List<PackResult>();

			using (var conn = new NpgsqlConnection(connString))
			{
				conn.Open();

				foreach (var contact in contacts)
				{
					try
					{
						using (var cmd = new NpgsqlCommand())
						{
							cmd.Connection = conn;
							cmd.CommandText = String.Format(@"INSERT INTO ""public"".""Product"" (""Name"", ""Code"", ""Id"") VALUES ('{0}', '{1}', '{2}')", contact.Name, contact.Code, contact.Id);
							cmd.ExecuteNonQuery();
						}

						result.Add(new PackResult() { IsSuccess = true, Id = contact.ERPId });
					}
					catch (Exception e)
					{
						result.Add(new PackResult() { IsSuccess = false, ErrorMessage = e.Message, Id = contact.ERPId });
					}
				}
			}

			return Ok(result);
		}
	}
}
