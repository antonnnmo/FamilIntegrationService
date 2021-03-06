﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FamilIntegrationService;
using FamilIntegrationService.Providers;
using FluentScheduler;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using ProcessingIntegrationService.Controllers;
using ProcessingIntegrationService.Coupons;
using ProcessingIntegrationService.Managers;

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
			services.AddControllers().AddNewtonsoftJson();
			services.AddMemoryCache();

			services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
			.AddJwtBearer(options =>
			{
				options.RequireHttpsMetadata = false;
				options.SaveToken = true;
				options.TokenValidationParameters = new TokenValidationParameters
				{
					ValidIssuer = "FamilIntegrationService",
					ValidAudience = "IntegrationUser",
					IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("gfdiog40-]kgf-043uo")),
					ValidateLifetime = true,
					ValidateIssuerSigningKey = true,
					ClockSkew = TimeSpan.Zero
				};
			}); ;

			services.AddMemoryCache();
			GlobalCacheReader.Cache.Set(GlobalCacheReader.CacheKeys.ProcessingUri, Configuration.GetValue<string>("ProcessingUri"));
            GlobalCacheReader.Cache.Set(GlobalCacheReader.CacheKeys.ProcessingSecret, Configuration.GetValue<string>("ProcessingSecret"));
            GlobalCacheReader.Cache.Set(GlobalCacheReader.CacheKeys.PersonalAreaToken, Configuration.GetValue<string>("PersonalAreaToken"));
            GlobalCacheReader.Cache.Set(GlobalCacheReader.CacheKeys.PersonalAreaLogin, Configuration.GetValue<string>("PersonalAreaLogin"));
            GlobalCacheReader.Cache.Set(GlobalCacheReader.CacheKeys.CardSynchronizationPeriod, Configuration.GetValue<int>("CardSynchronizationPeriod"));
            GlobalCacheReader.Cache.Set(GlobalCacheReader.CacheKeys.CardCleanPeriod, Configuration.GetValue<int>("CardCleanPeriod"));
            GlobalCacheReader.Cache.Set(GlobalCacheReader.CacheKeys.PersonalAreaPasword, Configuration.GetValue<string>("PersonalAreaPasword"));
            GlobalCacheReader.Cache.Set(GlobalCacheReader.CacheKeys.PersonalAreaUri, Configuration.GetValue<string>("PersonalAreaUri"));
            GlobalCacheReader.Cache.Set(GlobalCacheReader.CacheKeys.ConnectionString, Configuration.GetValue<string>("ConnectionString"));
			GlobalCacheReader.Cache.Set(GlobalCacheReader.CacheKeys.BPMLogin, Configuration.GetSection("BPMCredentials").GetValue<string>("login"));
			GlobalCacheReader.Cache.Set(GlobalCacheReader.CacheKeys.BPMPassword, Configuration.GetSection("BPMCredentials").GetValue<string>("password"));
			GlobalCacheReader.Cache.Set(GlobalCacheReader.CacheKeys.BPMUri, Configuration.GetSection("BPMCredentials").GetValue<string>("uri"));

			ProductManager.CreateTableIfNotExists();
			Promocode.CreateTableIfNotExists();
			AnswerTemplateCollection.CreateTableIfNotExist();
			CardController.CreateTableIfNotExist();
			CouponCache.CreateTableIfNotExist();

			JobManager.Initialize(new Scheduller());
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
		{
			loggerFactory.AddLog4Net();
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}

			app.UseHttpsRedirection();

			app.UseRouting();

			app.UseAuthentication();
			app.UseAuthorization();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers();
			});
		}
	}
}
