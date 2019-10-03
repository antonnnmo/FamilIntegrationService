using System;
using System.Collections.Generic;
using System.Text;

namespace FamilIntegrationCore.Models
{
	public class CardProcessingModel: BaseProcessingModel
	{
		public string Number { get; set; }
		public int State { get; set; }
		public string ContactId { get; set; }
        public bool IsMain { get; set; }
        public string CardId { get; set; }
	}
}
