using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace FamilServiceMonitoringApp.DB
{
	public class ContactInfoEvent
	{
		[Key]
		public Guid Id { get; set; }
		public DateTime Time { get; set; }
		public int Timeout { get; set; }
		public string EventName { get; set; }

		public bool IsError { get; set; }
	}
}
