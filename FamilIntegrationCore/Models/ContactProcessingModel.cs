using System;
using System.Collections.Generic;
using System.Text;

namespace FamilIntegrationCore.Models
{
	public class ContactProcessingModel: BaseProcessingModel
	{
		public string Name { get; set; }
		public string Phone { get; set; }

		#region PersonalArea params

		public string FirstName { get; set; }
		public string Surname { get; set; }
		public string MiddleName { get; set; }
		public string Email { get; set; }
		public string Gender { get; set; }
		public string Country { get; set; }
		public string City { get; set; }
		public string Address { get; set; }
		public string Birthday { get; set; }
		public string RequiresCorrection { get; set; }
		public string ThereAreEmptyFields { get; set; }
		public string PersDataProcAgreement { get; set; }
		public string ShopCode { get; set; }
		public string RegistrationDate { get; set; }

		#endregion
	}
}
