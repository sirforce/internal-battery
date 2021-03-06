﻿using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UpDiddyApi.Models;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Hangfire;
using UpDiddyApi.Workflow;
using Hangfire.SqlServer;
using System;
using UpDiddyLib.Helpers;
using UpDiddyLib.Shared;
using Microsoft.ApplicationInsights.SnapshotCollector;
using Microsoft.Extensions.Options;
using Microsoft.ApplicationInsights.AspNetCore;
using Microsoft.ApplicationInsights.Extensibility;
using Serilog;
using UpDiddyLib.Serilog.Sinks;
using Microsoft.Extensions.Logging;
using Serilog.Events;
using UpDiddyApi.ApplicationCore.Interfaces;
using UpDiddyApi.ApplicationCore.Services;
using UpDiddyApi.ApplicationCore.Services.AzureAPIManagement;
using System.Collections.Generic;
using UpDiddyApi.Authorization;
using Microsoft.AspNetCore.Authorization;
using UpDiddyApi.Helpers.SignalR;
using UpDiddyApi.Authorization.APIGateway;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.ApplicationCore.Repository;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNetCore.StaticFiles;
using UpDiddyApi.ApplicationCore.Services.CourseCrawling;
using UpDiddyApi.ApplicationCore.Services.Identity.Interfaces;
using UpDiddyApi.ApplicationCore.Services.Identity;
using UpDiddyApi.ApplicationCore.ActionFilter;
using Microsoft.AspNetCore.Mvc.Versioning;
using UpDiddyApi.ApplicationCore.Services.AzureSearch;
using G2Services = UpDiddyApi.ApplicationCore.Services.G2;
using G2Interfaces = UpDiddyApi.ApplicationCore.Interfaces.Business.G2;
using UpDiddyApi.ApplicationCore.Services.G2;
using UpDiddyApi.ApplicationCore.Interfaces.Business.HiringManager;
using UpDiddyApi.ApplicationCore.Services.HiringManager;
using UpDiddyApi.ApplicationCore.Interfaces.Business.B2B;
using UpDiddyApi.ApplicationCore.Services.B2B;
using UpDiddyApi.ApplicationCore.Services.Candidate;
using UpDiddyApi.ApplicationCore.Services.Admin;

namespace UpDiddyApi
{
    public class Startup
    {
        private bool _isHangfireProcessingServer;
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

            // if environment is set to LocalDevelopment then add user secrets
            if (env.IsEnvironment("LocalDevelopment"))
            {
                builder.AddUserSecrets<Startup>();
            }
            
            // if environment is set to anything other than local development, add secrets from the key vault
            var config = builder.Build();
            if (!env.IsEnvironment("LocalDevelopment"))
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
                    .SendGrid(LogEventLevel.Fatal, Configuration["SysEmail:NotifySystem:ApiKey"], Configuration["SysEmail:SystemErrorEmailAddress"])
                .Enrich.FromLogContext()
                .CreateLogger();

            // set the value indicating whether or not Hangfire will be processing jobs in this instance
            Boolean.TryParse(Configuration["Hangfire:IsProcessingServer"], out _isHangfireProcessingServer);
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddApiVersioning(options =>
            {
                options.ReportApiVersions = true;
                options.DefaultApiVersion = new ApiVersion(2, 0);
                options.AssumeDefaultVersionWhenUnspecified = true;
            });

            string domain = $"https://{Configuration["Auth0:Domain"]}/";
            services.AddSingleton<Serilog.ILogger>(Logger);
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.Authority = domain;
                options.Audience = Configuration["Auth0:ApiIdentifier"];
            })
            .AddAPIGatewayAuth(options => { });
            services.AddAuthorization(options =>
            {
                options.AddPolicy("IsRecruiterPolicy", policy => policy.Requirements.Add(new HasScopeRequirement(new string[] { "Recruiter" }, domain)));
                options.AddPolicy("IsCareerCircleAdmin", policy => policy.Requirements.Add(new HasScopeRequirement(new string[] { "Career Circle Administrator" }, domain)));
                options.AddPolicy("IsRecruiterOrAdmin", policy => policy.Requirements.Add(new HasScopeRequirement(new string[] { "Recruiter", "Career Circle Administrator" }, domain)));
                options.AddPolicy("IsHiringManager", policy => policy.Requirements.Add(new HasScopeRequirement(new string[] { "Hiring Manager" }, domain)));

            });

            services.AddSingleton<IAuthorizationHandler, HasScopeHandler>();

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
            //configure MimeTypeService
            var provider = new FileExtensionContentTypeProvider();
            services.AddSingleton<IMimeMappingService>(new MimeMappingService(provider));

            //configuring RepositoryWrapper class to implement repository pattern
            services.AddScoped<IRepositoryWrapper, RepositoryWrapper>();

            services.AddMemoryCache();
            // Add framework services.
            // the 'ignore' option for reference loop handling was implemented to prevent circular errors during serialization 
            // (e.g. SubscriberDto contains a collection of EnrollmentDto objects, and the EnrollmentDto object has a reference to a SubscriberDto)
            services.AddMvc(o =>
            {
                o.EnableEndpointRouting = true;
            })
            .AddJsonOptions(x => x.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);

            // Add SignalR
            services.AddSignalR();

            // Add Odata
            services.AddOData();

            // Add AutoMapper 
            services.AddAutoMapper(typeof(UpDiddyApi.Helpers.AutoMapperConfiguration));

            // Configure Hangfire as the client (note that queueing and scheduling is controlled with the existing 'IsPreliminary' flag in HangfireService.cs)
            var HangFireSqlConnection = Configuration["CareerCircleSqlConnection"];
            JobStorage.Current = new SqlServerStorage(HangFireSqlConnection);

            // Configure Hangfire Server (for processing jobs)
            if (_isHangfireProcessingServer)
            {
                services.AddHangfire(options => options.UseSqlServerStorage(HangFireSqlConnection));
                GlobalConfiguration.Configuration.UseSqlServerStorage(HangFireSqlConnection);
                services.AddHangfireServer(options =>
                {
                    options.Queues = new[] { "default" }; // only using the default queue for now
                });
            }

            #region Hangfire jobs

            // Daily job for future enrollments 
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

            if (_currentEnvironment.IsProduction())
            {
                // run the job crawl in production Monday through Friday once per day at 15:00 UTC
                RecurringJob.AddOrUpdate<ScheduledJobs>(x => x.JobDataMining(), "0 15 * * Mon,Tue,Wed,Thu,Fri");
            }

            // run the process in staging once a week on the weekend (Sunday 4 UTC)
            if (_currentEnvironment.IsStaging())
            {
                RecurringJob.AddOrUpdate<ScheduledJobs>(x => x.JobDataMining(), Cron.Weekly(DayOfWeek.Sunday, 4));
            }

            // LOCAL TESTING ONLY - DO NOT UNCOMMENT THIS CODE!
            // BackgroundJob.Enqueue<ScheduledJobs>(x => x.JobDataMining());

            // run job to look for un-indexed profiles and index them 
            int profileIndexerBatchSize = int.Parse(Configuration["CloudTalent:ProfileIndexerBatchSize"]);
            int profileIndexerIntervalInMinutes = int.Parse(Configuration["CloudTalent:ProfileIndexerIntervalInMinutes"]);
            RecurringJob.AddOrUpdate<ScheduledJobs>(x => x.CloudTalentIndexNewProfiles(profileIndexerBatchSize), Cron.MinuteInterval(profileIndexerIntervalInMinutes));

            // kick off the metered welcome email delivery process at five minutes past the hour every hour
            RecurringJob.AddOrUpdate<ScheduledJobs>(x => x.ExecuteLeadEmailDelivery(), Cron.Hourly());

            // kick off the job abandonment email delivery process
            RecurringJob.AddOrUpdate<ScheduledJobs>(x => x.ExecuteJobAbandonmentEmailDelivery(), Cron.Daily());

            // kick off the subscriber notification email reminder process every day at 12 UTC 
            RecurringJob.AddOrUpdate<ScheduledJobs>(x => x.SubscriberNotificationEmailReminder(), Cron.Daily(12));

            // sync job posting alerts between Hangfire and our database
            BackgroundJob.Enqueue<ScheduledJobs>(x => x.SyncJobPostingAlertsBetweenDbAndHangfire());

            // update the related job skill matrix table once per day at 5 UTC
            RecurringJob.AddOrUpdate<ScheduledJobs>(x => x.CacheRelatedJobSkillMatrix(), Cron.Daily(5));

            // generate the sitemap and save it to blob storage (do this only in staging and prod since we currently do not have a "development" instance for blob storage)
            if (_currentEnvironment.IsStaging())
                RecurringJob.AddOrUpdate<ScheduledJobs>(x => x.GenerateSiteMapAndSaveToBlobStorage(), Cron.Weekly(DayOfWeek.Sunday, 5));
            if (_currentEnvironment.IsProduction())
                RecurringJob.AddOrUpdate<ScheduledJobs>(x => x.GenerateSiteMapAndSaveToBlobStorage(), Cron.Daily(5));

            // kick sendgrid audit cleanup
            RecurringJob.AddOrUpdate<ScheduledJobs>(x => x.PurgeSendGridAuditRecords(), Cron.Daily());

       
            //disabled 2020.06.10 - the remote rest service does not seem to be working since 2020.04.13
            //string HiringSolvedCronExpression = $"*/{Configuration["HiringSolved:PollIntervalInSeconds"]} * * * * *";
            //RecurringJob.AddOrUpdate<ScheduledJobs>(x => x.GetHiringSolvedResumeParseUpdates(), HiringSolvedCronExpression);








            #endregion

            services.AddHttpClient(Constants.HttpGetClientName);
            services.AddHttpClient(Constants.HttpPostClientName);
            services.AddHttpClient(Constants.HttpPutClientName);
            services.AddHttpClient(Constants.HttpDeleteClientName);

            #region Add Custom Services
            services.AddTransient<ISovrenAPI, Sovren>();
            services.AddHttpClient<ISovrenAPI, Sovren>();
            services.AddTransient<ICloudStorage, AzureBlobStorage>();

            services.AddTransient<IAPIGateway, ManagementAPI>();
            services.AddHttpClient<IAPIGateway, ManagementAPI>();

            services.AddTransient<IB2CGraph, B2CGraphClient>();
            services.AddHttpClient<IB2CGraph, B2CGraphClient>();
            services.AddScoped<IProfileService, ApplicationCore.Services.ProfileService>();
            services.AddScoped<ISubscriberService, SubscriberService>();
            services.AddScoped<ISubscriberEducationalHistoryService, SubscriberEducationalHistoryService>();
            services.AddScoped<ISubscriberWorkHistoryService, SubscriberWorkHistoryService>();
            services.AddScoped<IReportingService, ReportingService>();
            services.AddScoped<IJobService, JobService>();
            services.AddScoped<ITrackingService, TrackingService>();
            services.AddScoped<IJobPostingService, JobPostingService>();
            services.AddScoped<IJobApplicationService, JobApplicationService>();
            services.AddScoped<ITraitifyService, TraitifyService>();
            services.AddScoped<IServiceOfferingService, ServiceOfferingService>();
            services.AddScoped<IServiceOfferingOrderService, ServiceOfferingOrderService>();
            services.AddScoped<IPromoCodeService, PromoCodeService>();
            services.AddScoped<IBraintreeService, BraintreeService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<ICourseService, CourseService>();
            services.AddScoped<ICompanyService, CompanyService>();
            services.AddScoped<IRecruiterService, RecruiterService>();
            services.AddScoped<ITaggingService, TaggingService>();
            services.AddScoped<ISubscriberNotificationService, SubscriberNotificationService>();
            services.AddScoped<ICourseCrawlingService, CourseCrawlingService>();
            services.AddScoped<IHangfireService, HangfireService>();
            services.AddScoped<IMemoryCacheService, MemoryCacheService>();
            services.AddScoped<IServiceOfferingPromoCodeRedemptionService, ServiceOfferingPromoCodeRedemptionService>();
            services.AddScoped<IJobFavoriteService, JobFavoriteService>();
            services.AddScoped<IJobSearchService, JobSearchService>();
            services.AddScoped<ICloudTalentService, CloudTalentService>();
            services.AddScoped<ISkillService, SkillService>();
            services.AddScoped<IFileDownloadTrackerService, FileDownloadTrackerService>();
            services.AddScoped<IJobAlertService, JobAlertService>();
            services.AddScoped<IResumeService, ResumeService>();
            services.AddScoped<IKeywordService, KeywordService>();
            services.AddScoped<ICountryService, CountryService>();
            services.AddScoped<IStateService, StateService>();
            services.AddScoped<IEmploymentTypeService, EmploymentTypeService>();
            services.AddScoped<IExperienceLevelService, ExperienceLevelService>();
            services.AddScoped<ICompensationTypeService, CompensationTypeService>();
            services.AddScoped<ISecurityClearanceService, SecurityClearanceService>();
            services.AddScoped<IContactService, ContactService>();
            services.AddScoped<ICourseFavoriteService, CourseFavoriteService>();
            services.AddScoped<IAvatarService, AvatarService>();
            services.AddScoped<ITraitifyServiceV2, TraitifyServiceV2>();
            services.AddScoped<ActionFilter>();
            services.AddScoped<ITalentService, TalentService>();
            services.AddScoped<ITalentFavoriteService, TalentFavoriteService>();
            services.AddScoped<ITalentNoteService, TalentNoteService>();
            services.AddScoped<IButterCMSService, ButterCMSService>();
            services.AddScoped<IPasswordResetRequestService, PasswordResetRequestService>();
            services.AddScoped<ITopicService, TopicService>();
            services.AddScoped<ISubscriberCourseService, SubscriberCourseService>();
            services.AddScoped<IOfferService, OfferService>();
            services.AddScoped<ICourseLevelService, CourseLevelService>();
            services.AddScoped<ICourseEnrollmentService, CourseEnrollmentService>();
            services.AddScoped<ISitemapService, SitemapService>();
            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<IEducationalDegreeTypeService, EducationalDegreeTypeService>();
            services.AddScoped<IEducationLevelService, EducationLevelService>();
            services.AddScoped<IIndustryService, IndustryService>();
            services.AddScoped<IPartnerService, PartnerService>();
            services.AddScoped<IGroupService, GroupService>();
            services.AddScoped<IPartnerService, PartnerService>();
            services.AddScoped<IAzureSearchService, AzureSearchService>();
            services.AddScoped<IReportsService, ReportsService>();
            services.AddScoped<ISendGridEventService, SendgridEventService>();
            services.AddScoped<ISubscriberEmailService, SubscriberEmailService>();
            services.AddScoped<IHiringSolvedService, HiringSolvedService>();
            services.AddScoped<IG2Service, G2Service>();
            services.AddScoped<G2Interfaces.IProfileService, G2Services.ProfileService>();
            services.AddScoped<G2Interfaces.IWishlistService, G2Services.WishlistService>();
            services.AddScoped<G2Interfaces.ICommentService, G2Services.CommentService>();
            services.AddScoped<ITagService, TagService>();
            services.AddScoped<ICityService, CityService>();
            services.AddScoped<IPostalService, PostalService>();
            services.AddScoped<ISendGridService, SendGridService>();
            services.AddScoped<IEmailTemplateService, EmailTemplateService>();
            services.AddScoped<IHubSpotService, HubSpotService>();
            services.AddScoped<IHiringManagerService, HiringManagerService>();
            services.AddScoped<IPipelineService, PipelineService>();
            services.AddTransient<IInterviewRequestService, InterviewRequestService>(); // y'all know most of these can be transient!!!
            services.AddTransient<ICandidatesService, CandidatesService>();
            services.AddCareerTalentPipelineService(Configuration.GetSection("CreateTalentPipeline"));
            services.AddCrossChq(Configuration.GetSection("CrossChq"));
            services.AddTransient<ICommuteDistancesService, CommuteDistancesService>();
            services.AddTransient<IAccountManagementService, AccountManagementService>();
            services.AddTransient<IVideoService, VideoService>();
            #endregion

            // Configure SnapshotCollector from application settings
            // TODO Uncomment test 
            //services.Configure<SnapshotCollectorConfiguration>(Configuration.GetSection(nameof(SnapshotCollectorConfiguration)));
            // Add SnapshotCollector telemetry processor.
            // TODO Uncomment test 
            //services.AddSingleton<ITelemetryProcessorFactory>(sp => new SnapshotCollectorTelemetryProcessorFactory(sp));

            // Add Redis session cahce
            services.AddStackExchangeRedisCache(options =>
            {
                options.InstanceName = Configuration.GetValue<string>("redis:name");
                options.Configuration = Configuration.GetValue<string>("redis:host");
            });

            services.AddRedisClient(Configuration.GetSection("redis"));

            // load file for tracking pixel as singleton to limit overhead
            services.AddSingleton<FileContentResult>(
                new FileContentResult(
                    Convert.FromBase64String(Configuration.GetValue<string>("Tracking:PixelContentBase64")),
                    Configuration.GetValue<string>("Tracking:PixelContentType")
                    )
                );

            // Uncomment the following line to enable extensive logging for Hangfire.
            // GlobalJobFilters.Filters.Add(new HangfireServerFilter(Configuration, Logger));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddSerilog(Logger);

            ScopeRead = Configuration["AzureAdB2C:ScopeRead"];
            ScopeWrite = Configuration["AzureAdB2C:ScopeWrite"];
            app.UseExceptionMiddleware();
            app.UseAuthentication();

            app.UseCors("Cors");

            // Configure the Hangfire dashboard only on the instance which is used for job processing
            if (_isHangfireProcessingServer)
            {
                app.UseHangfireDashboard("/dashboard", new DashboardOptions
                {
                    Authorization = new[] { new HangfireAuthorizationFilter(env, Configuration) }
                });
                app.UseHangfireServer();
            }

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
