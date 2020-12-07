using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MonitoringHelperService;
using MonitoringHelperService.Models;
using Newtonsoft.Json;
using Npgsql;

namespace MonitoringService.Controllers
{
    [ApiController]
    [Route("api/select")]
    public class EntityController : ControllerBase
    {
        private readonly ILogger<EntityController> _logger;

        public EntityController(ILogger<EntityController> logger)
        {
            _logger = logger;
        }

		[HttpPost("contact")]
		[Authorize]
		public ActionResult SelectContact([FromBody] Dictionary<string, object> request)
		{
			int count = 10;
			if (request.ContainsKey("count"))
			{
				int.TryParse(request.GetValueOrDefault("count").ToString(), out count);
			}
			var result = new List<Result>();
			using (var conn = new NpgsqlConnection(DBProvider.GetConnectionString()))
			{
				conn.Open();

				using (var cmd = new NpgsqlCommand())
				{
					cmd.Connection = conn;
					cmd.CommandText = string.Format(
						@"SELECT ""Id"" FROM ""public"".""Contact"" WHERE random() > 0.5 LIMIT {0}", count);
					using (var reader = cmd.ExecuteReader())
					{
						while (reader != null && reader.Read())
						{
							result.Add(new Result() { Id = reader.GetGuid(0) });
						}
					}

				}
				if (result.Count < count)
				{
					result.Clear();
					using (var cmd = new NpgsqlCommand())
					{
						cmd.Connection = conn;
						cmd.CommandText = string.Format(
							@"SELECT ""Id"" FROM ""public"".""Contact"" order by random() LIMIT {0}", count);
						using (var reader = cmd.ExecuteReader())
						{
							while (reader != null && reader.Read())
							{
								result.Add(new Result() { Id = reader.GetGuid(0) });
							}
						}

					}
				}
			}
			return Content(JsonConvert.SerializeObject(result, new JsonSerializerSettings
			{
				NullValueHandling = NullValueHandling.Ignore,
				DefaultValueHandling = DefaultValueHandling.Ignore
			}));
		}

		[HttpPost("product")]
		[Authorize]
		public ActionResult SelectProduct([FromBody] Dictionary<string, object> request)
		{
			int count = 10;
			if (request.ContainsKey("count"))
			{
				int.TryParse(request.GetValueOrDefault("count").ToString(), out count);
			}
			var result = new List<Result>();
			using (var conn = new NpgsqlConnection(DBProvider.GetConnectionString()))
			{
				conn.Open();

				using (var cmd = new NpgsqlCommand())
				{
					cmd.Connection = conn;
					cmd.CommandText = string.Format(
						@"SELECT ""Code"" FROM ""public"".""Product"" WHERE random() > 0.5 LIMIT {0}", count);
					using (var reader = cmd.ExecuteReader())
					{
						while (reader != null && reader.Read())
						{
							result.Add(new Result() { Code = reader.GetString(0) });
						}
					}

				}
				if (result.Count < count)
				{
					result.Clear();
					using (var cmd = new NpgsqlCommand())
					{
						cmd.Connection = conn;
						cmd.CommandText = string.Format(
							@"SELECT ""Code"" FROM ""public"".""Product"" order by random() LIMIT {0}", count);
						using (var reader = cmd.ExecuteReader())
						{
							while (reader != null && reader.Read())
							{
								result.Add(new Result() { Code = reader.GetString(0) });
							}
						}

					}
				}
			}
			return Content(JsonConvert.SerializeObject(result, new JsonSerializerSettings
			{
				NullValueHandling = NullValueHandling.Ignore,
				DefaultValueHandling = DefaultValueHandling.Ignore
			}));
		}

		[HttpPost("shop")]
		[Authorize]
		public ActionResult SelectShop([FromBody] Dictionary<string, object> request)
		{
			int count = 10;
			if (request.ContainsKey("count"))
			{
				int.TryParse(request.GetValueOrDefault("count").ToString(), out count);
			}
			var result = new List<Result>();
			using (var conn = new NpgsqlConnection(DBProvider.GetConnectionString()))
			{
				conn.Open();

				using (var cmd = new NpgsqlCommand())
				{
					cmd.Connection = conn;
					cmd.CommandText = string.Format(
						@"SELECT ""Code"" FROM ""public"".""Shop"" WHERE random() > 0.5 LIMIT {0}", count);
					using (var reader = cmd.ExecuteReader())
					{
						while (reader != null && reader.Read())
						{
							result.Add(new Result() { Code = reader.GetString(0) });
						}
					}

				}
				if (result.Count < count)
				{
					result.Clear();
					using (var cmd = new NpgsqlCommand())
					{
						cmd.Connection = conn;
						cmd.CommandText = string.Format(
							@"SELECT ""Code"" FROM ""public"".""Shop"" order by random() LIMIT {0}", count);
						using (var reader = cmd.ExecuteReader())
						{
							while (reader != null && reader.Read())
							{
								result.Add(new Result() { Code = reader.GetString(0) });
							}
						}

					}
				}
			}
			return Content(JsonConvert.SerializeObject(result, new JsonSerializerSettings
			{
				NullValueHandling = NullValueHandling.Ignore,
				DefaultValueHandling = DefaultValueHandling.Ignore
			}));
		}

	}
}
