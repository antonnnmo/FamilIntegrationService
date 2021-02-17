using System;
using System.Reflection;

namespace FamilServiceMonitoringApp
{
	public static class Logger
	{
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(Assembly.GetEntryAssembly(), "LOGGER");

		public static void LogError(string message)
		{
			log.Error(message);

		}

		public static void LogError(string message, params string[] parameters)
		{
			log.ErrorFormat(message, parameters);
		}

		public static void LogError(string v, Exception e)
		{
			log.Error(v, e);
		}

		public static void LogInfo(string v, string result)
		{
			log.Info(String.Format("{0} {1}", v, result));
		}
	}
}
