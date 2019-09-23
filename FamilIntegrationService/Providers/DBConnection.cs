using System;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;

namespace FamilIntegrationService.Providers
{
	public class DBConnection : IDisposable
	{
		private string _connectionString;
		SqlConnection sqlConnection;

		public static string ConnectionString { get; set; }

		public DBConnection(string connectionString)
		{
			_connectionString = connectionString;
		}

		public SqlDataReader Execute(string command, params object[] parameters)
		{
			sqlConnection = new SqlConnection(_connectionString);
			sqlConnection.Open();
			try
			{
				using (var sqlCommand = new SqlCommand(Validate(command, parameters), sqlConnection))
				{
					sqlCommand.CommandTimeout = 5000;
					return sqlCommand.ExecuteReader();
				}
			}
			catch (Exception e)
			{
				throw new Exception(Validate(command, parameters) + " " + e.Message);
			}
		}

		public int UploadFile(string command, Stream stream, params object[] parameters)
		{
			using (var sqlConnection = new SqlConnection(_connectionString))
			{

				sqlConnection.Open();
				using (var sqlCommand = new SqlCommand(Validate(command, parameters), sqlConnection))
				{
					sqlCommand.Parameters.Add(new SqlParameter("@Data", new SqlBytes(stream)));
					return sqlCommand.ExecuteNonQuery();
				}
			}
		}

		public int ExecuteNonQuery(string command, params object[] parameters)
		{
			using (var sqlConnection = new SqlConnection(_connectionString))
			{
				sqlConnection.Open();
				using (var sqlCommand = new SqlCommand(Validate(command, parameters), sqlConnection))
				{
					return sqlCommand.ExecuteNonQuery();
				}
			}
		}

		public int ExecuteNonQueryWithFile(string command, byte[] data, params object[] parameters)
		{
			using (var sqlConnection = new SqlConnection(_connectionString))
			{
				sqlConnection.Open();
				using (var sqlCommand = new SqlCommand(Validate(command, parameters), sqlConnection))
				{
					var param = sqlCommand.Parameters.Add("@Data", SqlDbType.VarBinary);
					param.Value = data;
					return sqlCommand.ExecuteNonQuery();
				}
			}
		}

		public T ExecuteScalar<T>(string command, T defValue, params object[] parameters)
		{
			using (var sqlConnection = new SqlConnection(_connectionString))
			{
				sqlConnection.Open();
				using (var sqlCommand = new SqlCommand(Validate(command, parameters), sqlConnection))
				{
					var res = sqlCommand.ExecuteScalar();
					if (res == null || res is DBNull) return defValue;
					return (T)res;
				}
			}
		}

		private string Validate(string command, object[] parameters)
		{
			if (parameters == null) return command;
			return String.Format(command, parameters.Select(p => (p == null || String.IsNullOrEmpty(p.ToString())) ? String.Empty : p.ToString().Replace("'", "''")).ToArray());
		}

		public void Dispose()
		{
			if (sqlConnection != null)
			{
				sqlConnection.Dispose();
				sqlConnection.Close();
			}
		}
	}
}
