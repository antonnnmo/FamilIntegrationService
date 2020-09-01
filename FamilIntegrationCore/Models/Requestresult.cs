using System;
using System.Collections.Generic;
using System.Text;

namespace FamilIntegrationCore.Models
{
	public class RequestResult
	{
		public bool IsSuccess { get; set; }
		public bool IsTimeout { get; set; }
		public string ResponseStr { get; set; }
	}
}
