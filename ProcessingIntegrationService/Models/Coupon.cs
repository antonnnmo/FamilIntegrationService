using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProcessingIntegrationService.Models
{
	public class Coupon
	{
		public List<CouponText> Texts { get; set; }
		public List<PromotionDto> Promotions { get; set; }

		public bool IsActive { get; set; }
		public string Name { get; set; }
		public Guid Id { get; set; }
	}
}
