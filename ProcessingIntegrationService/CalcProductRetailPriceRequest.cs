using Newtonsoft.Json;
using System.Collections.Generic;

namespace ProcessingIntegrationService
{
	public class CalcProductRetailPriceRequest
	{
		[JsonProperty]
		public List<string> ProductCodes { get; set; }
	}
}
