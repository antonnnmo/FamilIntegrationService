using System.Collections.Generic;

namespace ProcessingIntegrationService.Models
{
	public class CouponResponse
	{
		public string Name { get; set; }
		public List<CouponTextResponse> Texts { get; set; }
		public class CouponTextResponse
		{
			public int Index { get; set; }
			public string Text { get; set; }
		}
		
	}
}
