using FamilIntegrationService.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace FamilIntegrationCore.Models
{
	public class ProductCategory : BaseIntegrationObject
	{
		[JsonProperty]
		public string Name { get; set; }
		[JsonProperty]
		public string Direction { get; set; }
	}
}
