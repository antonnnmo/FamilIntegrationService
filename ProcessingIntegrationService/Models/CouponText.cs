using System;

namespace ProcessingIntegrationService.Models
{
	public class CouponText
	{
		public Guid Id { get; set; }
		public string Text { get; set; }
		public int Order { get; set; }
	}
}