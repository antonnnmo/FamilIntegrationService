using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace FamilIntegrationCore.Models
{
	public class Product : BaseIntegrationObject
	{
		[JsonProperty]
		public string Name { get; set; }

		[JsonProperty]
		public string Code { get; set; }

		[JsonProperty]
		public bool IsArchived { get; set; }

		[JsonProperty]
		public string Direction { get; set; }

		[JsonProperty]
		public string SubCategory { get; set; }

		[JsonProperty]
		public string Group { get; set; }

		[JsonProperty]
		public string Brand { get; set; }

		[JsonProperty]
		public string BrandType { get; set; }

		[JsonProperty]
		public decimal RecommendedRetailPrice { get; set; }

		[JsonProperty]
		public string Provider { get; set; }

		[JsonProperty]
		public string Size { get; set; }

		[JsonProperty]
		public string Category { get; set; }
	}
}
