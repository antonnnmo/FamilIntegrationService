using FamilServiceMonitoringApp.DB;
using FluentScheduler;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;

namespace FamilServiceMonitoringApp.Schedulle
{ 
	public class TableCreatingJob : IJob
	{
		private readonly object _lock = new object();
		private readonly ServiceDBContext _dbContext;
		private bool _shuttingDown;

		public TableCreatingJob(ServiceDBContext dbContext)
		{
			_dbContext = dbContext;
		}

		public void Execute()
		{
			try
			{
				lock (_lock)
				{
					var connection = _dbContext.Database.GetDbConnection();

                    bool isConnectionClosed = connection.State == ConnectionState.Closed;

                    if (isConnectionClosed)
                    {
                        connection.Open();
                    }

                    var existingTableNames = new List<string>();
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "SELECT table_name from INFORMATION_SCHEMA.TABLES WHERE table_type = 'base table'";

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                existingTableNames.Add(reader.GetString(0).ToLowerInvariant());
                            }
                        }
                    }

					var tableNames = new string[] { TableNameManager.GetCurrentDayTableName(), TableNameManager.GetTableNameByDay(DateTime.UtcNow.AddDays(1)), TableNameManager.GetTableNameByDay(DateTime.UtcNow.AddDays(2)) };

					foreach(var newTableName in tableNames) 
					{
						if (!existingTableNames.Contains(newTableName.ToLower())) 
						{
							using (var createCommand = connection.CreateCommand())
							{
								createCommand.CommandText = $@"CREATE TABLE [dbo].[{newTableName}](
	[Id] [uniqueidentifier] NOT NULL,
	[Time] [datetime2](7) NOT NULL,
	[Timeout] [int] NOT NULL,
	[EventName] [nvarchar](max) NULL,
	[IsError] [bit] NOT NULL,
 CONSTRAINT [PK_{newTableName}] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY];
ALTER TABLE [dbo].[{newTableName}] ADD  DEFAULT (CONVERT([bit],(0))) FOR [IsError];";
								createCommand.ExecuteNonQuery();
							}
						}
					}

					

					if (isConnectionClosed)
                    {
                        connection.Close();
                    }
                }
			}
			finally
			{
			}
		}

		public void Stop(bool immediate)
		{
			lock (_lock)
			{
				_shuttingDown = true;
			}

		}
	}
}