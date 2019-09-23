using System;
using System.Data.SqlClient;
using System.IO;

namespace FamilIntegrationService.Providers
{
	public class DBConnectionProvider : IDisposable
	{
		DBConnection connection;
		public SqlDataReader Execute(string sqlText, params object[] parameters)
		{
			connection = GetConnection();
			return connection.Execute(sqlText, parameters);
		}

		public static int ExecuteNonQuery(string sqlText, params object[] parameters)
		{
			var connection = GetConnection();
			return connection.ExecuteNonQuery(sqlText, parameters);
		}

		public static int UploadFile(string sqlText, Stream stream, params object[] parameters)
		{
			var connection = GetConnection();
			return connection.UploadFile(sqlText, stream, parameters);
		}

		public static T ExecuteScalar<T>(string sqlText, T defValue, params object[] parameters)
		{
			var connection = GetConnection();
			return connection.ExecuteScalar<T>(sqlText, defValue, parameters);
		}

		private static DBConnection GetConnection()
		{
			var connectionString = String.Empty;
			if (GlobalCacheReader.GetValue(GlobalCacheReader.CacheKeys.SqlConnectionString, out connectionString))
			{
				return new DBConnection(connectionString);
			}
			else
			{
				throw new Exception("Empty sql connection string");
			}
		}

		public void Dispose()
		{
			if (connection != null)
			{
				connection.Dispose();
			}
		}
	}
}
