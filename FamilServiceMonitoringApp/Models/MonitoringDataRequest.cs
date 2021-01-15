using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FamilServiceMonitoringApp.Models
{
	public class MonitoringDataRequest
	{
		public string Period { get; set; }
		public DateTime? Start { get; set; }
		public DateTime? End { get; set; }
	}
}
