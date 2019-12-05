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
using Newtonsoft.Json.Serialization;
using Npgsql;
using ProcessingIntegrationService.Managers;
using ProcessingIntegrationService.Models;

namespace ProcessingIntegrationService.Controllers
{
	public class SettingsRequest
	{
		public string Value { get; set; }
	}

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

		

		[HttpPost("LoadCalculateResponseTemplatePrefix")]
		public ActionResult LoadCalculateResponseTemplatePrefix([FromBody]SettingsRequest req)
		{
			AnswerTemplateCollection.CalculateResponseTemplatePrefix = req.Value;
			return Ok();
		}

		[HttpPost("SendProductPrice")]
		public ActionResult SendProductPrice([FromBody]SendProductPriceRequest request)
		{
			if (request != null)
			{
				new ProductManager().ChangeProductPrice(request);
				return Ok(new { Result = "success" });
			}

			return BadRequest(new { Result = "parameter errors" });
		}
		[HttpPost("SendPromocodePool")]
		public ActionResult SendPromocodePool([FromBody]SendPromocodePoolRequest request)
		{
			if (request != null)
			{
				Promocode.ChangePool(request);
				return Ok(new { Result = "success" });
			}

			return BadRequest(new { Result = "parameter errors" });
		}

		[HttpPost("LoadContactPack")]
		[Authorize]
		public ActionResult LoadContactPack([FromBody]IEnumerable<ContactProcessingModel> contacts)
		{
            return new ContactManager().LoadPack(contacts);
        }

		[HttpPost("LoadPrimaryContactPack")]
		[Authorize]
		public ActionResult LoadPrimaryContactPack([FromBody]IEnumerable<ContactProcessingModel> contacts)
		{
            return new ContactManager().LoadPrimaryPack(contacts);
        }

		[HttpPost("LoadPrimaryShopPack")]
		[Authorize]
		public ActionResult LoadPrimaryShopPack([FromBody]IEnumerable<ShopProcessingModel> contacts)
		{
            return new ShopManager().LoadPrimaryPack(contacts);
        }

		[HttpPost("LoadShopPack")]
		[Authorize]
		public ActionResult LoadShopPack([FromBody]IEnumerable<ShopProcessingModel> contacts)
		{
            return new ShopManager().LoadPack(contacts);
        }

		[HttpPost("LoadPrimaryProductPack")]
		[Authorize]
		public ActionResult LoadPrimaryProductPack([FromBody]IEnumerable<ProductProcessingModel> contacts)
		{
            return new ProductManager().LoadPrimaryPack(contacts);
        }

		[HttpPost("LoadProductPack")]
		[Authorize]
		public ActionResult LoadProductPack([FromBody]IEnumerable<ProductProcessingModel> contacts)
		{
            return new ProductManager().LoadPack(contacts);
        }

        [HttpPost("LoadPrimaryCardPack")]
        [Authorize]
        public ActionResult LoadPrimaryCardPack([FromBody]IEnumerable<CardProcessingModel> cards)
        {
            return new CardManager().LoadPrimaryPack(cards);
        }

        [HttpPost("LoadCardPack")]
        [Authorize]
        public ActionResult LoadCardPack([FromBody]IEnumerable<CardProcessingModel> cards)
        {
            return new CardManager().LoadPack(cards);
        }
    }
}
