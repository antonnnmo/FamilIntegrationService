using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FamilIntegrationService.Providers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ProcessingIntegrationService
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
			services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

			services.AddMemoryCache();
			GlobalCacheReader.Cache.Set(GlobalCacheReader.CacheKeys.ProcessingUri, Configuration.GetValue<string>("ProcessingUri"));
			GlobalCacheReader.Cache.Set(GlobalCacheReader.CacheKeys.ConnectionString, Configuration.GetValue<string>("ConnectionString"));
			GlobalCacheReader.Cache.Set(GlobalCacheReader.CacheKeys.BPMLogin, Configuration.GetSection("BPMCredentials").GetValue<string>("login"));
			GlobalCacheReader.Cache.Set(GlobalCacheReader.CacheKeys.BPMPassword, Configuration.GetSection("BPMCredentials").GetValue<string>("password"));
			GlobalCacheReader.Cache.Set(GlobalCacheReader.CacheKeys.BPMUri, Configuration.GetSection("BPMCredentials").GetValue<string>("uri"));
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
		{
			loggerFactory.AddLog4Net();

			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}
			else
			{
				// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
				app.UseHsts();
			}

			app.UseHttpsRedirection();
			app.UseMvc();
		}
	}
}
