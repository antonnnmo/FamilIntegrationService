using System.Collections.Generic;
using System.Diagnostics;
using FamilIntegrationService;
using LoyaltyMiddleware.Loyalty;
using LoyaltyMiddleware.MiddlewareHandlers;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RedmondLoyaltyMiddleware.MiddlewareHandlers;

namespace LoyaltyMiddleware.Controllers
{
	[Route("purchase")]
	[ApiController]
	public class PurchaseController : ControllerBase
	{
		public PurchaseController()
		{
		}

		[HttpGet("ping")]
		public string ping ()
		{
			return "123";
		}

		[HttpPost("calculate")]
		public ActionResult Calculate([FromBody] Dictionary<string, object> request)
		{
			return HandleRequest(request, "calculate", new PreCalculateHandler(), new CalculateHandler());
		}

		[HttpPost("confirm")]
		public ActionResult Confirm([FromBody] Dictionary<string, object> request)
		{
			return HandleRequest(request, "confirm", null, new ConfirmHandler());
		}

		private ActionResult HandleRequest(Dictionary<string, object> request, string method, IPreRequestHandler preRequestHandler, IRequestHandler afterRequestHandler)
		{
			System.Diagnostics.Stopwatch timerGlobal = new Stopwatch();

			timerGlobal.Start();
			var authHeader = HttpContext.Request.Headers["Authorization"];
			if (authHeader.Count == 0) return Unauthorized();

			Logger.LogInfo($"started preRequest {method}", "");

			Dictionary<string, object> additionalResponseData = null;
			if (preRequestHandler != null)
			{
				var result = preRequestHandler.GetHandledRequest(request);
				request = result.Request;
				additionalResponseData = result.AdditionalResponseData;
			}

			Logger.LogInfo($"finished preRequest {method}", "");

			Logger.LogInfo($"started {method} request", "");
			System.Diagnostics.Stopwatch timer = new Stopwatch();

			timer.Start();

			var response = new ProcessingManager().PRRequest($"purchase/{method}", JsonConvert.SerializeObject(request), authHeader);

			timer.Stop();
			Logger.LogInfo($"finished {method} request: time: {timer.ElapsedMilliseconds}" , "");

			if (response.IsSuccess)
			{
				HttpContext.Response.Headers.Add("Content-Type", "application/json");
				var responseData = JsonConvert.DeserializeObject<Dictionary<string, object>>(response.ResponseStr);
				var handledResponse = afterRequestHandler.GetHandledResponse(request, responseData, additionalResponseData);
				timerGlobal.Stop();
				Logger.LogInfo($"finished {method} Global request: time: {timerGlobal.ElapsedMilliseconds}", "");
				return Ok(handledResponse);
			}
			else if (response.IsUnathorized) 
			{
				timerGlobal.Stop();
				Logger.LogInfo($"finished {method} Global request: time: {timerGlobal.ElapsedMilliseconds}", "");
				return Unauthorized();
			}
			else
			{
				timerGlobal.Stop();
				Logger.LogInfo($"finished {method} Global request: time: {timerGlobal.ElapsedMilliseconds}", "");
				return BadRequest(response.ResponseStr);
			}
		}
	}
}
