using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProcessingIntegrationService.Models
{
	public class CardGenerateResponse
	{
        [JsonProperty("data")]
        public CardGenerateData Data { get; set; }

        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("error")]
        public object Error { get; set; }

        [JsonProperty("warnings")]
        public string[] Warnings { get; set; }
    }

    public class CardGenerateData
    {
        [JsonProperty("number")]
        public string Number { get; set; }
    }
}
