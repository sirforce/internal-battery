﻿using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UpDiddyApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Cors;
using AutoMapper;
using UpDiddyLib.Dto;
using UpDiddyApi.Helpers;
using Microsoft.AspNetCore.Mvc;
using Hangfire;
using UpDiddyApi.Workflow;
using Hangfire.SqlServer;
using System;
using UpDiddyLib.Helpers;
using Polly;
using Polly.Extensions.Http;
using System.Net.Http;
using UpDiddyLib.Helpers;
using UpDiddyLib.Shared;
using Microsoft.ApplicationInsights.SnapshotCollector;
using Microsoft.Extensions.Options;
using Microsoft.ApplicationInsights.AspNetCore;
using Microsoft.ApplicationInsights.Extensibility;
using Serilog;
using Serilog.Sinks.ApplicationInsights;
using UpDiddyLib.Serilog.Sinks;
using Microsoft.Extensions.Logging;
using Serilog.Events;
using UpDiddyApi.ApplicationCore.Interfaces;
using UpDiddyApi.ApplicationCore.Services;
using UpDiddyApi.ApplicationCore.Services.AzureAPIManagement;
using System.Collections.Generic;
using UpDiddyApi.ApplicationCore.Interfaces;
using System.Security.Claims;
using UpDiddyApi.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using UpDiddyApi.Helpers.SignalR;
using UpDiddyApi.Authorization.APIGateway;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.ApplicationCore.Repository;
using Microsoft.AspNet.OData.Extensions;

namespace UpDiddyApi
{
    public class Startup
    {
        private readonly IHostingEnvironment _currentEnvironment;
        public static string ScopeRead;
        public static string ScopeWrite;
        public IConfigurationRoot Configuration { get; set; }

        public Serilog.ILogger Logger { get; }

        public ISysEmail SysEmail { get; }

        public Startup(IHostingEnvironment env, IConfiguration configuration)
        {
            // set the current environment so that we can access it in ConfigureServices
            _currentEnvironment = env;

            // Note: please refer to UpDiddyDbContext if this logic needs to be updated (configuration)
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            // if environment is set to development then add user secrets
            if (env.IsDevelopment())
            {
                builder.AddUserSecrets<Startup>();
            }

            // if environment is set to staging or production then add vault keys
            var config = builder.Build();
            if (env.IsStaging() || env.IsProduction())
            {
                builder.AddAzureKeyVault(config["Vault:Url"],
                    config["Vault:ClientId"],
                    config["Vault:ClientSecret"],
                    new KeyVaultSecretManager());
            }

            Configuration = builder.Build();

            SysEmail = new SysEmail(Configuration);

            // directly add Application Insights and SendGrid to access Key Vault Secrets
            Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(Configuration)
                .WriteTo.ApplicationInsightsTraces(Configuration["APPINSIGHTS_INSTRUMENTATIONKEY"])
                .WriteTo
                    .SendGrid(LogEventLevel.Fatal, Configuration["SysEmail:Transactional:ApiKey"], Configuration["SysEmail:SystemErrorEmailAddress"])
                .Enrich.FromLogContext()
                .CreateLogger();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddSingleton<Serilog.ILogger>(Logger);

            services.AddAuthentication(options =>
            {
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(jwtOptions =>
            {
                jwtOptions.Authority = $"https://login.microsoftonline.com/tfp/{Configuration["AzureAdB2C:Tenant"]}/{Configuration["AzureAdB2C:Policy"]}/v2.0/";
                jwtOptions.Audience = Configuration["AzureAdB2C:ClientId"];
                jwtOptions.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = AuthenticationFailed
                };
            })
            .AddAPIGatewayAuth(options => { });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("IsRecruiterPolicy", policy => policy.AddRequirements(new GroupRequirement(new string[] { "Recruiter" })));
                options.AddPolicy("IsCareerCircleAdmin", policy => policy.AddRequirements(new GroupRequirement(new string[] { "Career Circle Administrator" })));
                options.AddPolicy("IsUserAdmin", policy => policy.AddRequirements(new GroupRequirement(new string[] { "Career Circle User Admin" })));
                options.AddPolicy("IsRecruiterOrAdmin", policy => policy.AddRequirements(new GroupRequirement(new string[] { "Recruiter", "Career Circle Administrator" })));
            });
            services.AddSingleton<IAuthorizationHandler, GroupAuthorizationHandler>();

            // Get the connection string from the Azure secret vault
            var SqlConnection = Configuration["CareerCircleSqlConnection"];
            services.AddDbContextPool<UpDiddyDbContext>(options => options.UseSqlServer(SqlConnection));

            // Add Dependency Injection for the configuration object
            services.AddSingleton<IConfiguration>(Configuration);
            // Add System Email   
            services.AddSingleton<ISysEmail>(new SysEmail(Configuration));

            List<string> origins = Configuration.GetSection("Cors:Origins").Get<List<string>>();
            // Shows UseCors with CorsPolicyBuilder.
            services.AddCors(o => o.AddPolicy("Cors", builder =>
            {
                builder.WithOrigins(origins.ToArray())
                       .AllowCredentials()
                       .AllowAnyMethod()
                       .AllowAnyHeader();
            }));

            //configuring RepositoryWrapper class to implement repository pattern
            services.AddScoped<IRepositoryWrapper, RepositoryWrapper>();

            // Add framework services.
            // the 'ignore' option for reference loop handling was implemented to prevent circular errors during serialization 
            // (e.g. SubscriberDto contains a collection of EnrollmentDto objects, and the EnrollmentDto object has a reference to a SubscriberDto)
            services.AddMvc().AddJsonOptions(x => x.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);

            // Add SignalR
            services.AddSignalR();

            // Add Odata
            services.AddOData();

            // Add AutoMapper 
            services.AddAutoMapper(typeof(UpDiddyApi.Helpers.AutoMapperConfiguration));

            // Configure Hangfire 
            var HangFireSqlConnection = Configuration["CareerCircleSqlConnection"];
            services.AddHangfire(x => x.UseSqlServerStorage(HangFireSqlConnection));
            // Have the workflow monitor run every minute 
            JobStorage.Current = new SqlServerStorage(HangFireSqlConnection);
            RecurringJob.AddOrUpdate<ScheduledJobs>(x => x.ReconcileFutureEnrollments(), Cron.Daily);
            // Batch job for updating woz student course progress 
            int CourseProgressSyncIntervalInHours = 12;
            int.TryParse(Configuration["Woz:CourseProgressSyncIntervalInHours"].ToString(), out CourseProgressSyncIntervalInHours); 
            RecurringJob.AddOrUpdate<ScheduledJobs>(x => x.UpdateAllStudentsProgress(), Cron.HourInterval(CourseProgressSyncIntervalInHours));
 
            // PromoCodeRedemption cleanup
            int promoCodeRedemptionCleanupScheduleInMinutes = 5;
            int promoCodeRedemptionLookbackInMinutes = 30;
            int.TryParse(Configuration["PromoCodeRedemptionCleanupScheduleInMinutes"].ToString(), out promoCodeRedemptionCleanupScheduleInMinutes);
            int.TryParse(Configuration["PromoCodeRedemptionLookbackInMinutes"].ToString(), out promoCodeRedemptionLookbackInMinutes);
            RecurringJob.AddOrUpdate<ScheduledJobs>(x => x.DoPromoCodeRedemptionCleanup(promoCodeRedemptionLookbackInMinutes), Cron.MinuteInterval(promoCodeRedemptionCleanupScheduleInMinutes));

            // remove TinyIds from old CampaignPartnerContact records
            RecurringJob.AddOrUpdate<ScheduledJobs>(x => x.DeactivateCampaignPartnerContacts(), Cron.Daily());

            // run the process in production Monday through Friday once every 2 hours between 11 and 23 UTC
            if (_currentEnvironment.IsProduction())
                RecurringJob.AddOrUpdate<ScheduledJobs>(x => x.JobDataMining(), "0 11,13,15,17,19,21,23 * * Mon,Tue,Wed,Thu,Fri");

            // run the process in staging once a week on the weekend (Sunday 4 UTC)
            if (_currentEnvironment.IsStaging())
                RecurringJob.AddOrUpdate<ScheduledJobs>(x => x.JobDataMining(), Cron.Weekly(DayOfWeek.Sunday, 4));

            // run job to look for un-indexed profiles and index them 
            int profileIndexerBatchSize = int.Parse(Configuration["CloudTalent:ProfileIndexerBatchSize"]);
            int profileIndexerIntervalInMinutes = int.Parse(Configuration["CloudTalent:ProfileIndexerIntervalInMinutes"]);
            RecurringJob.AddOrUpdate<ScheduledJobs>(x => x.CloudTalentIndexNewProfiles(profileIndexerBatchSize), Cron.MinuteInterval(profileIndexerIntervalInMinutes) );


            // use for local testing only - DO NOT UNCOMMENT AND COMMIT THIS CODE!
            // BackgroundJob.Enqueue<ScheduledJobs>(x => x.JobDataMining());

            // kick off the metered welcome email delivery process at five minutes past the hour every hour
            RecurringJob.AddOrUpdate<ScheduledJobs>(x => x.ExecuteLeadEmailDelivery(), Cron.Hourly());

            // kick off the job abandonment email delivery process
            RecurringJob.AddOrUpdate<ScheduledJobs>(x => x.ExecuteJobAbandonmentEmailDelivery(), Cron.Daily());

            // Add Polly 
            // Create Policies  
            int PollyRetries = int.Parse(Configuration["Polly:Retries"]);
            int PollyTimeoutInSeconds = int.Parse(Configuration["Polly:PollyTimeoutInSeconds"]);

            // Create a timeout policy that will prevent  api  get calls from taking more that PollyTimeoutInSeconds 
            // in total.  This timeout is inclusive of the intitial get call and any subsequent polly retries.  For example
            // if PollyTimeoutInSeconds = 8 and PollyRetries = 5 and a get call responds with an error at 4 seconds, the 
            // operation will fail after the second retry since the PollyTimeoutInSeconds has been exceeded.
            var ApiGetTimeoutPolicy = Policy.TimeoutAsync(PollyTimeoutInSeconds);
            // Create retry policy          
            var ApiGetRetryPolicy = HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetryAsync(PollyRetries, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
            // Combine timeout and retry policies to create Get policy       
            var ApiGetPolicy = ApiGetTimeoutPolicy.WrapAsync(ApiGetRetryPolicy);

            // Define a policy without retries for non idempotenic operations
            // FMI: https://www.stevejgordon.co.uk/httpclientfactory-using-polly-for-transient-fault-handling
            var ApiPostPolicy = Policy.NoOpAsync().AsAsyncPolicy<HttpResponseMessage>();
            var ApiPutPolicy = Policy.NoOpAsync().AsAsyncPolicy<HttpResponseMessage>();
            var ApiDeletePolicy = Policy.NoOpAsync().AsAsyncPolicy<HttpResponseMessage>();

            services.AddHttpClient(Constants.HttpGetClientName)
              .AddPolicyHandler(ApiGetPolicy);

            services.AddHttpClient(Constants.HttpPostClientName)
                          .AddPolicyHandler(ApiPostPolicy);

            services.AddHttpClient(Constants.HttpPutClientName)
                          .AddPolicyHandler(ApiPutPolicy);

            services.AddHttpClient(Constants.HttpDeleteClientName)
              .AddPolicyHandler(ApiDeletePolicy);


            #region Add Custom Services
            services.AddTransient<ISovrenAPI, Sovren>();
            services.AddHttpClient<ISovrenAPI, Sovren>();
            services.AddTransient<ICloudStorage, AzureBlobStorage>();

            services.AddTransient<IAPIGateway, ManagementAPI>();
            services.AddHttpClient<IAPIGateway, ManagementAPI>();

            services.AddTransient<IB2CGraph, B2CGraphClient>();
            services.AddHttpClient<IB2CGraph, B2CGraphClient>();

            services.AddScoped<ISubscriberService, SubscriberService>();
            services.AddScoped<IReportingService, ReportingService>();
            services.AddScoped<IJobService, JobService>();
            services.AddScoped<ITrackingService, TrackingService>();
            services.AddScoped<IJobPostingService, JobPostingService>();
            services.AddScoped<IJobApplicationService, JobApplicationService>();


            services.AddScoped<ICompanyService, CompanyService>();
            services.AddScoped<IRecruiterService, RecruiterService>();
            services.AddScoped<ITaggingService, TaggingService>();
            #endregion

            // Configure SnapshotCollector from application settings
            // TODO Uncomment test 
            //services.Configure<SnapshotCollectorConfiguration>(Configuration.GetSection(nameof(SnapshotCollectorConfiguration)));
            // Add SnapshotCollector telemetry processor.
            // TODO Uncomment test 
            //services.AddSingleton<ITelemetryProcessorFactory>(sp => new SnapshotCollectorTelemetryProcessorFactory(sp));

            // Add Redis session cahce
            services.AddDistributedRedisCache(options =>
            {
                options.InstanceName = Configuration.GetValue<string>("redis:name");
                options.Configuration = Configuration.GetValue<string>("redis:host");
            });

            // load file for tracking pixel as singleton to limit overhead
            services.AddSingleton<FileContentResult>(
                new FileContentResult(
                    Convert.FromBase64String(Configuration.GetValue<string>("Tracking:PixelContentBase64")),
                    Configuration.GetValue<string>("Tracking:PixelContentType")
                    )
                );
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddSerilog(Logger);

            ScopeRead = Configuration["AzureAdB2C:ScopeRead"];
            ScopeWrite = Configuration["AzureAdB2C:ScopeWrite"];

            app.UseAuthentication();

            app.UseCors("Cors");

            app.UseHangfireDashboard("/dashboard", new DashboardOptions
            {
                Authorization = new[] { new HangfireAuthorizationFilter(env, Configuration) }
            });
            app.UseHangfireServer();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Version}/{action=Get}/{id?}");
                
                // Odata
                routes.Filter().OrderBy().Count();
                routes.EnableDependencyInjection();
            });

            // Added for SignalR
            app.UseSignalR(routes =>
            {
                routes.MapHub<ClientHub>("/clienthub");
            });

        }

        private Task AuthenticationFailed(AuthenticationFailedContext arg)
        {
            // For debugging purposes only!
            var s = $"AuthenticationFailed: {arg.Exception.Message}";
            arg.Response.ContentLength = s.Length;
            arg.Response.Body.Write(Encoding.UTF8.GetBytes(s), 0, s.Length);
            return Task.FromResult(0);
        }

        private class SnapshotCollectorTelemetryProcessorFactory : ITelemetryProcessorFactory
        {
            private readonly IServiceProvider _serviceProvider;

            public SnapshotCollectorTelemetryProcessorFactory(IServiceProvider serviceProvider) =>
                _serviceProvider = serviceProvider;

            public ITelemetryProcessor Create(ITelemetryProcessor next)
            {
                var snapshotConfigurationOptions = _serviceProvider.GetService<IOptions<SnapshotCollectorConfiguration>>();
                return new SnapshotCollectorTelemetryProcessor(next, configuration: snapshotConfigurationOptions.Value);
            }
        }
    }
}
