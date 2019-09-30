using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FamilIntegrationCore.Models;
using FamilIntegrationService;
using FamilIntegrationService.Models;
using FamilIntegrationService.Providers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace ProcessingIntegrationService.Controllers
{
	[Route("api/Main")]
	[ApiController]
	public class MainController : ControllerBase
	{
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
		[Authorize]
		public ActionResult LoadContactPack([FromBody]IEnumerable<ContactProcessingModel> contacts)
		{
			if (contacts == null) return BadRequest("Ошибка передачи аргументов");
			var result = new List<PackResult>();

			using (var conn = new NpgsqlConnection(DBProvider.GetConnectionString()))
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
		[Authorize]
		public ActionResult LoadPrimaryContactPack([FromBody]IEnumerable<ContactProcessingModel> contacts)
		{
			if (contacts == null) return BadRequest("Ошибка передачи аргументов");

			var queries = new List<string>();

			var sb = new StringBuilder();
			sb.AppendLine(@"INSERT INTO ""public"".""Contact"" (""Name"", ""Phone"", ""Id"") VALUES ");

			sb.AppendLine(String.Join(",", contacts.Select(c => String.Format(@"('{0}', '{1}', '{2}')", c.Name, c.Phone, c.Id))));

			using (var conn = new NpgsqlConnection(DBProvider.GetConnectionString()))
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
		[Authorize]
		public ActionResult LoadPrimaryShopPack([FromBody]IEnumerable<ShopProcessingModel> contacts)
		{
			if (contacts == null) return BadRequest("Ошибка передачи аргументов");

			var queries = new List<string>();

			var sb = new StringBuilder();
			sb.AppendLine(@"INSERT INTO ""public"".""Shop"" (""Name"", ""Code"", ""Id"") VALUES ");

			sb.AppendLine(String.Join(",", contacts.Select(c => String.Format(@"('{0}', '{1}', '{2}')", c.Name, c.Code, c.Id))));

			using (var conn = new NpgsqlConnection(DBProvider.GetConnectionString()))
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
		[Authorize]
		public ActionResult LoadShopPack([FromBody]IEnumerable<ShopProcessingModel> contacts)
		{
			if (contacts == null) return BadRequest("Ошибка передачи аргументов");
			var result = new List<PackResult>();

			using (var conn = new NpgsqlConnection(DBProvider.GetConnectionString()))
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
		[Authorize]
		public ActionResult LoadPrimaryProductPack([FromBody]IEnumerable<ProductProcessingModel> contacts)
		{
			if (contacts == null) return BadRequest("Ошибка передачи аргументов");
			try
			{
				var queries = new List<string>();

				var sb = new StringBuilder();
				sb.AppendLine(@"INSERT INTO ""public"".""Product"" (""Name"", ""Code"", ""Id"") VALUES ");

				sb.AppendLine(String.Join(",", contacts.Select(c => String.Format(@"('{0}', '{1}', '{2}')", c.Name, c.Code, c.Id))));

				using (var conn = new NpgsqlConnection(DBProvider.GetConnectionString()))
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
		[Authorize]
		public ActionResult LoadProductPack([FromBody]IEnumerable<ProductProcessingModel> contacts)
		{
			if (contacts == null) return BadRequest("Ошибка передачи аргументов");
			var result = new List<PackResult>();

			using (var conn = new NpgsqlConnection(DBProvider.GetConnectionString()))
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
