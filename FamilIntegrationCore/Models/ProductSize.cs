using FamilIntegrationService.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace FamilIntegrationCore.Models
{
	public class ProductSizeList : IntegrationObjectRequest
	{
		[JsonProperty]
		public List<ProductSize> Objects { get; set; }
	}

	public class ProductSize : BaseIntegrationObject
	{
		[JsonProperty]
		public string Name { get; set; }
	}
}
