using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FamilIntegrationCore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProcessingIntegrationService.Managers;

namespace PersonalAreaIntegrationService.Controllers
{
	[Route("api/main")]
	[ApiController]
	public class MainController : ControllerBase
	{
		[HttpGet]
		public ActionResult Get()
		{
			return Ok("ok");
		}

		[HttpPost("LoadContactPack")]
		//[Authorize]
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
