using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FamilServiceMonitoringApp.DB;
using FamilServiceMonitoringApp.Extensions;
using FamilServiceMonitoringApp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FamilServiceMonitoringApp.Controllers
{
	[Route("api/Processing")]
	[ApiController]
	public class ProcessingController : ControllerBase
	{
		private readonly ServiceDBContext _dbContext;

		public ProcessingController(ServiceDBContext dbContext)
		{
			_dbContext = dbContext;
		}

		[HttpPost("Calculate")]
		public ActionResult Calculate([FromBody] MonitoringDataRequest request)
		{
			var eventsCount = _dbContext.Events.Count();
			var percent = 100.0 * _dbContext.Events.Count(e => e.Timeout > 3000) / eventsCount;

			IEnumerable<EventVw> events = null;
			IEnumerable<Event> initialEvents = null;

			if (request == null || request.Period == "minute")
			{
				initialEvents = _dbContext.Events.Where(e => e.Time >= DateTime.UtcNow.AddMinutes(-1)).ToList();
				events = initialEvents.OrderByDescending(e => e.Time).Select(e => new EventVw() { Time = e.Time.ToString("HH:mm:ss"), Timeout = e.Timeout, IsError = e.IsError });
			}
			else if(request == null || request.Period == "5minute")
			{
				initialEvents = _dbContext.Events.Where(e => e.Time >= DateTime.UtcNow.AddMinutes(-5)).ToList();
				events = initialEvents.OrderByDescending(e => e.Time).Select(e => new EventVw() { Time = e.Time.ToString("HH:mm:ss"), Timeout = e.Timeout, IsError = e.IsError });
			}
			else if (request.Period == "hour")
			{
				var start = DateTime.UtcNow.AddHours(-1);
				start = start.AddMinutes(-start.Minute % 5).AddSeconds(-start.Second);

				initialEvents = _dbContext.Events.Where(e => e.Time >= start).ToList();
				events = initialEvents.Select(e => new { e.Time, e.Timeout, e.IsError })
					.OrderBy(e => e.Time)
					.GroupBy(e => e.Time.ToString("HH:mm"))
					.ToDictionary(e => e.Key, e => new EventVw() { Timeout = e.Max(d => d.Timeout), IsError = e.Any(d => d.IsError), Time = e.Key } )
					.Select(d => d.Value);
			}
			else if (request.Period == "day")
			{
				initialEvents = _dbContext.Events.Where(e => e.Time >= DateTime.UtcNow.AddDays(-1)).ToList();
				events = initialEvents
					.OrderBy(e => e.Time)
					.GroupBy(e => e.Time.ToString("HH:mm"))
					.ToDictionary(e => e.Key, e => new { Timeout = e.Max(d => d.Timeout), Min = e.Min(d => d.Timeout), IsError = e.Any(d => d.IsError), Time = e.Key })
					.Select(d => d.Value)
					.Select(e => new EventVw() { Time = e.Time, Timeout = e.Timeout > 3000 ? e.Timeout : e.Min, IsError = e.IsError });
			}

			var startTime = events.Min(e => e.Time);
			var endTime = events.Max(e => e.Time);
			var periodCount = initialEvents.Count();
			var periodSum = initialEvents.Sum(e => e.Timeout);

			return Ok(new
			{
				start = startTime,
				end = endTime,
				events = events.Select(e => e.Timeout),
				accessibility = events.Select(e => e.IsError ? 0 : 2999),
				ExcessRate = percent,
				EventsCount = periodCount,
				AllEventsCount = eventsCount,
				Avg = periodSum / periodCount
			});
		}

		internal class EventVw
		{
			public string Time { get; internal set; }
			public int Timeout { get; internal set; }
			public bool IsError { get; internal set; }
		}
	}
}
