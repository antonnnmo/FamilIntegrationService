using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FamilServiceMonitoringApp.DB
{
	public class TableNameManager
	{
		public static string GetCurrentDayTableName() 
		{
			return GetTableNameByDay(DateTime.UtcNow);
		}

		public static string GetTableNameByDay(DateTime date)
		{
			return $"Events_{date.ToString("dd_MM_yyyy")}";
		}
	}
}
