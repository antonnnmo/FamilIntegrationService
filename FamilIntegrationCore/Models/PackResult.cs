using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace FamilIntegrationService.Models
{
	public class PackResults
	{
		[JsonProperty]
		public List<PackResult> IntegratePackResult { get; set; }
	}

	public class PrimaryIntegratePackResponse
	{
		[JsonProperty]
		public List<PackResult> PrimaryIntegratePackResult { get; set; }
	}

	public class PackResult
	{
		[JsonProperty]
		public bool IsTimeout { get; set; }
		[JsonProperty]
		public string Id { get; set; }
		[JsonProperty]
		public bool IsSuccess { get; set; }
		[JsonProperty]
		public string ErrorMessage { get; set; }
		[JsonProperty]
		public string ContactId { get; set; }
        public List<CustomField> CustomFields { get; set; }

        public string GetCorrectId()
        {
            return (Id ?? "").Replace("'", "''");
        }
	}

    public class CustomField
    {
        [JsonProperty]
        public string Name { get; set; }
        [JsonProperty]
        public string Value { get; set; }

        public CustomField() { }

        public CustomField(string name, string value)
        {
            Name = name;
            Value = value;
        }

        public override string ToString()
        {
            return string.Format("{0}:{1}", Name, Value);
        }
    }
}
