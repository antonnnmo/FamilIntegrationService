using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MonitoringHelperService;
using MonitoringHelperService.Models;
using Newtonsoft.Json;
using Npgsql;

namespace MonitoringService.Controllers
{
    [ApiController]
    [Route("select")]
    public class EntityController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;

        public EntityController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }

		//[HttpGet]
		//public IEnumerable<WeatherForecast> Get()
		//{
		//    var rng = new Random();
		//    return Enumerable.Range(1, 5).Select(index => new WeatherForecast
		//    {
		//        Date = DateTime.Now.AddDays(index),
		//        TemperatureC = rng.Next(-20, 55),
		//        Summary = Summaries[rng.Next(Summaries.Length)]
		//    })
		//    .ToArray();
		//}

		[HttpPost("contact")]
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
