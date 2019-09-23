using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace FamilIntegrationCore.Models
{
	public class ProductGroup : BaseIntegrationObject
	{
		[JsonProperty]
		public string Name { get; set; }

		[JsonProperty]
		public string Category { get; set; }
	}
}
