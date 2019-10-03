using FamilIntegrationService.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace FamilIntegrationCore.Models
{
	public class BaseIntegrationObject
	{
		[JsonProperty]
		public Guid Id { get; set; }
		[JsonProperty]
		public string ERPId { get; set; }
        public List<CustomField> CustomFields { get; set; }

        public BaseIntegrationObject()
        {
            CustomFields = new List<CustomField>();
        }
    }
}
