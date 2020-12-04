using System;
using System.ComponentModel.DataAnnotations;

namespace FamilServiceMonitoringApp.DB
{
	public class Event
	{
		[Key]
		public Guid Id { get; set; }
		public DateTime Time { get; set; }
		public int Timeout { get; set; }
		public string EventName { get; set; }

		public bool IsError { get; set; }
	}
}