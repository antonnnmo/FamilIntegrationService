using System;
using System.Collections.Generic;
using System.Text;

namespace FamilIntegrationCore.Models
{
	public class SendPromocodePoolRequest
	{
		public string Name { get; set; }
		public string Description { get; set; }
		public Guid Id { get; set; }
	}
}
