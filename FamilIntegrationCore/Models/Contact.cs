using FamilIntegrationCore.Models;
using Newtonsoft.Json;

namespace FamilIntegrationService.Models
{
	public class Contact: BaseIntegrationObject
	{
		[JsonProperty]
		public string BirthDay { get; set; }
		[JsonProperty]
		public string ErrorMessage { get; set; }
		[JsonProperty]
		public string CreatedOn { get; set; }
		[JsonProperty]
		public string FirstName { get; set; }
		[JsonProperty]
		public string Surname { get; set; }
		[JsonProperty]
		public string MiddleName { get; set; }
		[JsonProperty]
		public string Email { get; set; }
		[JsonProperty]
		public string Phone { get; set; }
		[JsonProperty]
		public bool IsMan { get; set; }
		[JsonProperty]
		public string Country { get; set; }
		[JsonProperty]
		public string City { get; set; }
		[JsonProperty]
		public string Address { get; set; }
		[JsonProperty]
		public string SmrNearestMetroStation { get; set; }
		[JsonProperty]
		public string RegistrationDate { get; set; }
		[JsonProperty]
		public string ShopCode { get; set; }
		[JsonProperty]
		public string ContactStatus { get; set; }
		[JsonProperty]
		public bool RequiresCorrection { get; set; }
		[JsonProperty]
		public bool PersDataProcAgreement { get; set; }
		[JsonProperty]
		public string ThereAreEmptyFields { get; set; }
		[JsonProperty]
		public decimal BonusBalance { get; set; }
		[JsonProperty]
		public int Type { get; set; }

		public string ToJson()
		{
			return JsonConvert.SerializeObject(this);
		}

		public static Contact FromJson(string json) => JsonConvert.DeserializeObject<Contact>(json);
	}
}
