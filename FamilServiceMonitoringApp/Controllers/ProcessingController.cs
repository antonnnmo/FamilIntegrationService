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

			//if (request == null || request.Period == "minute")
			//{
			//	initialEvents = _dbContext.Events.Where(e => e.Time >= DateTime.UtcNow.AddMinutes(-1)).ToList();
			//	events = initialEvents.OrderByDescending(e => e.Time).Select(e => new EventVw() { Time = e.Time.ToString("HH:mm:ss"), Timeout = e.Timeout, IsError = e.IsError });
			//}
			//else if(request == null || request.Period == "5minute")
			//{
			//	initialEvents = _dbContext.Events.Where(e => e.Time >= DateTime.UtcNow.AddMinutes(-5)).ToList();
			//	events = initialEvents.OrderByDescending(e => e.Time).Select(e => new EventVw() { Time = e.Time.ToString("HH:mm:ss"), Timeout = e.Timeout, IsError = e.IsError });
			//}
			//else 
			if (request == null || request.Period == "hour")
			{
				var start = DateTime.UtcNow.AddHours(-1);
				start = start.AddMinutes(-start.Minute % 5).AddSeconds(-start.Second);

				initialEvents = _dbContext.Events.Where(e => e.Time >= start).ToList();
				events = initialEvents.Select(e => new { e.Time, e.Timeout, e.IsError, e.EventName })
					.OrderBy(e => e.Time)
					.GroupBy(e => new { Time = e.Time.ToString("HH:mm"), Event = e.EventName })
					.ToDictionary(e => e.Key, e => new EventVw() { Timeout = e.Max(d => d.Timeout), IsError = e.Any(d => d.IsError), Time = e.Key.Time, Event = e.Key.Event })
					.Select(d => d.Value);
			}
			else if (request.Period == "day")
			{
				initialEvents = _dbContext.Events.Where(e => e.Time >= DateTime.UtcNow.AddDays(-1)).ToList();
				events = initialEvents
					.OrderBy(e => e.Time)
					.GroupBy(e => new { Time = e.Time.ToString("HH"), Event = e.EventName })
					.ToDictionary(e => e.Key, e => new { Timeout = e.Max(d => d.Timeout), IsError = e.Any(d => d.IsError), Time = e.Key.Time, Event = e.Key.Event })
					.Select(d => d.Value)
					.Select(e => new EventVw() { Time = e.Time + ":00", Timeout = e.Timeout, IsError = e.IsError, Event = e.Event });
			}
			else if (request.Period == "custom") 
			{
				var start = request.Start;
				var end = request.End;
				if (!request.Start.HasValue && !request.End.HasValue)
				{
					end = DateTime.UtcNow;
					start = DateTime.UtcNow.AddDays(-1);
					initialEvents = _dbContext.Events.Where(e => e.Time >= DateTime.UtcNow.AddDays(-1)).ToList();
				}
				else if (request.Start.HasValue && !request.End.HasValue)
				{
					end = DateTime.UtcNow;
					initialEvents = _dbContext.Events.Where(e => e.Time >= start).ToList();
				}
				else if (!request.Start.HasValue && request.End.HasValue)
				{
					start = _dbContext.Events.Min(e => e.Time);
					initialEvents = _dbContext.Events.Where(e => e.Time <= end).ToList();
				}
				else if (request.Start.HasValue && request.End.HasValue)
				{
					initialEvents = _dbContext.Events.Where(e => e.Time >= start && e.Time <= end).ToList();
				}

				var diff = (end - start).Value;
				if (diff.TotalDays > 2)
				{
					events = initialEvents
						.OrderBy(e => e.Time)
						.GroupBy(e => new { Time = e.Time.ToString("dd.MM.yyyy"), Event = e.EventName })
						.ToDictionary(e => e.Key, e => new { Timeout = e.Max(d => d.Timeout), IsError = e.Any(d => d.IsError), Time = e.Key.Time, Event = e.Key.Event })
						.Select(d => d.Value)
						.Select(e => new EventVw() { Time = e.Time, Timeout = e.Timeout, IsError = e.IsError, Event = e.Event });
				}
				else if (diff.TotalHours > 2)
				{
					events = initialEvents
						.OrderBy(e => e.Time)
						.GroupBy(e => new { Time = e.Time.ToString("dd.MM HH"), Event = e.EventName })
						.ToDictionary(e => e.Key, e => new { Timeout = e.Max(d => d.Timeout), IsError = e.Any(d => d.IsError), Time = e.Key.Time, Event = e.Key.Event })
						.Select(d => d.Value)
						.Select(e => new EventVw() { Time = e.Time + ":00", Timeout = e.Timeout, IsError = e.IsError, Event = e.Event });
				}
				else 
				{
					events = initialEvents.Select(e => new { e.Time, e.Timeout, e.IsError, e.EventName })
						.OrderBy(e => e.Time)
						.GroupBy(e => new { Time = e.Time.ToString("HH:mm"), Event = e.EventName } )
						.ToDictionary(e => e.Key, e => new EventVw() { Timeout = e.Max(d => d.Timeout), IsError = e.Any(d => d.IsError), Time = e.Key.Time, Event = e.Key.Event })
						.Select(d => d.Value);
				}
			}

			var startTime = events.Min(e => e.Time);
			var endTime = events.Max(e => e.Time);
			var periodCount = initialEvents.Count();
			var periodSum = initialEvents.Sum(e => e.Timeout);
			var allSum = _dbContext.Events.Sum(e => e.Timeout);

			var labels = events.Select(e => e.Time).Distinct();

			return Ok(new
			{
				start = startTime,
				end = endTime,
				events = labels.Select(l => events.Where(e => e.Event == "Calculate" && e.Time == l).FirstOrDefault()?.Timeout ?? 0),
				ciEvents = labels.Select(l => events.Where(e => e.Event == "ContactInfo" && e.Time == l).FirstOrDefault()?.Timeout ?? 0),
				labels = labels,
				accessibility = events.Select(e => e.IsError ? 0 : 2999),
				ExcessRate = percent,
				EventsCount = periodCount,
				AllEventsCount = eventsCount,
				AvgByPeriod = periodCount == 0 ? 0 : periodSum / periodCount,
				Avg = eventsCount == 0? 0 : allSum / eventsCount,
				ExcessRateByPeriod = periodCount == 0 ? 0 : 100.0 * initialEvents.Count(e => e.Timeout > 3000) / periodCount
		});
		}

		internal class EventVw
		{
			public string Time { get; internal set; }
			public string Event { get; internal set; }
			public int Timeout { get; internal set; }
			public bool IsError { get; internal set; }
			public string Label { get; internal set; }
		}
	}
}
