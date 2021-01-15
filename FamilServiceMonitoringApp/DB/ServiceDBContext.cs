using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FamilServiceMonitoringApp.DB
{
	public class ServiceDBContext: DbContext
	{
		private IConfiguration _configuration;

		public DbSet<Event> Events { get; set; }
		public DbSet<ContactInfoEvent> ContactInfoEvents { get; set; }

        public ServiceDBContext(DbContextOptions<ServiceDBContext> options, IConfiguration configuration) : base(options)
        {
            _configuration = configuration;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(_configuration.GetConnectionString("Database"));
            }
        }
    }
}
