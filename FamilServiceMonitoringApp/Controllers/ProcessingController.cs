using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FamilServiceMonitoringApp.DB;
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
		public ActionResult Calculate() 
		{
			var eventsCount = _dbContext.Events.Count();
			var percent = 100.0*_dbContext.Events.Count(e => e.Timeout > 3000)/ eventsCount;

			var events = _dbContext.Events.OrderByDescending(e => e.Time).Take(60).Select(e => new { e.Time, e.Timeout, e.IsError }).ToList().OrderBy(e => e.Time);
			var startTime = events.Min(e => e.Time).ToString("HH:mm");
			var endTime = events.Max(e => e.Time).ToString("HH:mm");
			return Ok(new { start = startTime, end = endTime, events = events.Select(e => e.Timeout), accessibility = events.Select(e => e.IsError ? 0 : 1500), ExcessRate = percent, EventsCount = eventsCount });
		}
	}
}
