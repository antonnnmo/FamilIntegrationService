using FamilServiceMonitoringApp.DB;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using FluentScheduler;
using FamilServiceMonitoringApp.Schedulle;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace FamilServiceMonitoringApp
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{

			services.AddControllersWithViews();

			// In production, the React files will be served from this directory
			services.AddSpaStaticFiles(configuration =>
			{
				configuration.RootPath = "ClientApp/build";
			});

			services.AddSingleton(_ => Configuration);
			services.AddDbContext<ServiceDBContext>();
			services.AddTransient<CalculateJob>();
			services.AddTransient<ContactInfoJob>();
			services.AddTransient<TableClearingJob>();
			services.AddTransient<CacheJob>();

			services.AddMemoryCache();
			GlobalCacheReader.Cache.Set(GlobalCacheReader.CacheKeys.CacheInterval, Configuration.GetValue<int>("CacheInterval"));
			GlobalCacheReader.Cache.Set(GlobalCacheReader.CacheKeys.HelperServiceUrl, Configuration.GetValue<string>("HelperServiceUrl"));
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ServiceDBContext dbContext, ILoggerFactory loggerFactory)
		{
			dbContext.Database.Migrate();

			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}
			else
			{
				app.UseExceptionHandler("/Error");
				// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
				app.UseHsts();
			}

			loggerFactory.AddLog4Net();
			app.UseHttpsRedirection();
			app.UseStaticFiles();
			app.UseSpaStaticFiles();

			app.UseRouting();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllerRoute(
					name: "default",
					pattern: "{controller}/{action=Index}/{id?}");
			});

			app.UseSpa(spa =>
			{
				spa.Options.SourcePath = "ClientApp";

				if (env.IsDevelopment())
				{
					spa.UseReactDevelopmentServer(npmScript: "start");
				}
			});

			JobManager.Initialize(new Scheduller(app.ApplicationServices));
		}
	}
}
