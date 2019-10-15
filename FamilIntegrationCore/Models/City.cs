using FamilIntegrationCore.Models;
using Newtonsoft.Json;

namespace FamilIntegrationService.Models
{
    public class City : BaseIntegrationObject
    {
        [JsonProperty]
        public string ErrorMessage { get; set; }
        [JsonProperty]
        public string CreatedOn { get; set; }
        [JsonProperty]
        public string Name { get; set; }
        [JsonProperty]
        public string TimeZone { get; set; }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }

        public static City FromJson(string json) => JsonConvert.DeserializeObject<City>(json);
    }
}
