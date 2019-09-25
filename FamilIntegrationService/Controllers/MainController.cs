using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace FamilIntegrationService.Controllers
{
	[Route("api/Main")]
	[ApiController]
	public class MainController : ControllerBase
	{
		[HttpGet]
		public ActionResult StartIntegration()
		{
			new Task(() => { new ContactManager().Execute(); }).Start();

			return Ok();
		}

		[HttpGet("Primary")]
		public ActionResult StartPrimaryIntegration()
		{
			new Task(() => { new ContactManager().ExecutePrimary(); }).Start();

			return Ok();
		}

		[HttpGet("PrimaryProductSize")]
		public ActionResult StartPrimaryProductSize()
		{
			new Task(() => { new ProductSizeManager().ExecutePrimary(); }).Start();

			return Ok();
		}

		[HttpGet("ProductSize")]
		public ActionResult StartProductSize()
		{
			new Task(() => { new ProductSizeManager().Execute(); }).Start();

			return Ok();
		}

		[HttpGet("PrimaryProductGroup")]
		public ActionResult StartPrimaryProductGroup()
		{
			new Task(() => { new ProductGroupManager().ExecutePrimary(); }).Start();

			return Ok();
		}

		[HttpGet("ProductGroup")]
		public ActionResult StartProductGroup()
		{
			new Task(() => { new ProductGroupManager().Execute(); }).Start();

			return Ok();
		}

		[HttpGet("PrimaryProductTag")]
		public ActionResult StartPrimaryProductTag()
		{
			new Task(() => { new ProductTagManager().ExecutePrimary(); }).Start();

			return Ok();
		}

		[HttpGet("ProductTag")]
		public ActionResult StartProductTag()
		{
			new Task(() => { new ProductTagManager().Execute(); }).Start();

			return Ok();
		}

		[HttpGet("PrimaryProductCategory")]
		public ActionResult StartPrimaryProductCategory()
		{
			new Task(() => { new ProductCategoryManager().ExecutePrimary(); }).Start();

			return Ok();
		}

		[HttpGet("ProductCategory")]
		public ActionResult StartProductCategory()
		{
			new Task(() => { new ProductCategoryManager().Execute(); }).Start();

			return Ok();
		}

		[HttpGet("PrimaryProductSubCategory")]
		public ActionResult StartPrimaryProductSubCategory()
		{
			new Task(() => { new ProductSubCategoryManager().ExecutePrimary(); }).Start();

			return Ok();
		}

		[HttpGet("ProductSubCategory")]
		public ActionResult StartProductSubCategory()
		{
			new Task(() => { new ProductSubCategoryManager().Execute(); }).Start();

			return Ok();
		}

		[HttpGet("PrimaryBrandType")]
		public ActionResult StartPrimaryBrandType()
		{
			new Task(() => { new BrandTypeManager().ExecutePrimary(); }).Start();

			return Ok();
		}

		[HttpGet("BrandType")]
		public ActionResult StartBrandType()
		{
			new Task(() => { new BrandTypeManager().Execute(); }).Start();

			return Ok();
		}

		[HttpGet("PrimaryShop")]
		public ActionResult StartPrimaryShop()
		{
			new Task(() => { new ShopManager().ExecutePrimary(); }).Start();

			return Ok();
		}

		[HttpGet("Shop")]
		public ActionResult StartShop()
		{
			new Task(() => { new ShopManager().Execute(); }).Start();

			return Ok();
		}

		[HttpGet("PrimarySMS")]
		public ActionResult StartPrimarySMS()
		{
			new Task(() => { new SMSManager().ExecutePrimary(); }).Start();

			return Ok();
		}

		[HttpGet("SMS")]
		public ActionResult StartSMS()
		{
			new Task(() => { new SMSManager().Execute(); }).Start();

			return Ok();
		}

		[HttpGet("PrimaryProduct")]
		public ActionResult StartPrimaryProduct()
		{
			new Task(() => { new ProductManager().ExecutePrimary(); }).Start();

			return Ok();
		}

		[HttpGet("PrimaryPurchase")]
		public ActionResult StartPrimaryPurchase()
		{
			new Task(() => { new PurchaseManager().ExecutePrimary(); }).Start();

			return Ok();
		}

		[HttpGet("Product")]
		public ActionResult StartProduct()
		{
			new Task(() => { new ProductManager().Execute(); }).Start();

			return Ok();
		}

		[HttpGet("Purchase")]
		public ActionResult StartPurchase()
		{
			new Task(() => { new PurchaseManager().Execute(); }).Start();

			return Ok();
		}

		[HttpGet("PrimaryProductInPurchase")]
		public ActionResult StartPrimaryProductInPurchase()
		{
			new Task(() => { new ProductInPurchaseManager().ExecutePrimary(); }).Start();

			return Ok();
		}

		[HttpGet("PrimaryPaymentInPurchase")]
		public ActionResult StartPrimaryPaymentInPurchase()
		{
			new Task(() => { new PaymentInPurchaseManager().ExecutePrimary(); }).Start();

			return Ok();
		}

		[HttpGet("Brand")]
		public ActionResult StartBrand()
		{
			new Task(() => { new BrandManager().Execute(); }).Start();

			return Ok();
		}
	}
}
