using Newtonsoft.Json;
using System;

namespace FamilIntegrationService
{
	public class AnswerTemplate
	{
		[JsonProperty]
		public string PrefixText { get; set; }
		[JsonProperty]
		public string SuffixText { get; set; }
		[JsonProperty]
		public decimal From { get; set; }
		[JsonProperty]
		public decimal To { get; set; }
		[JsonProperty]
		public decimal Price { get; set; }
		[JsonProperty]
		public DateTime Start { get; set; }
		[JsonProperty]
		public DateTime End { get; set; }
		[JsonProperty]
		public bool IsFirstTextBlock { get; set; }
	}
}
