using Hangfire;
using Hangfire.Dashboard.Dark;
using Hangfire.Mongo;
using Hangfire.Mongo.Migration.Strategies;
using Hangfire.Mongo.Migration.Strategies.Backup;
using HangfireBasicAuthenticationFilter;
using MongoDB.Driver;
using TuxfamilyScraper.Library.Data;

namespace TuxfamilyScraper.Library.Services;

public static class HangfireService
{
    public static void ConfigureHangfireServices(this WebApplicationBuilder builder)
    {
        var mongoUrlBuilder =
            new MongoUrlBuilder(builder.Configuration.GetValue<string>("ConnectionStrings:HangfireMongoDb"));
        var mongoClient = new MongoClient(mongoUrlBuilder.ToMongoUrl());

        builder.Services.AddHangfire(configuration => configuration
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UseMongoStorage(mongoClient, mongoUrlBuilder.DatabaseName, new MongoStorageOptions
            {
                MigrationOptions = new MongoMigrationOptions
                {
                    MigrationStrategy = new MigrateMongoMigrationStrategy(),
                    BackupStrategy = new CollectionMongoBackupStrategy()
                },
                Prefix = "tfs.hangfire",
                CheckConnection = false,
                CheckQueuedJobsStrategy = CheckQueuedJobsStrategy.Poll
            })
            .UseDarkDashboard()
        );

        

        builder.Services.AddHangfireServer(serverOptions =>
        {
            serverOptions.ServerName = "TuxFamilyScraper";
        });
    }

    public static void UseHangfireDashboard(this WebApplication app)
    {
        app.UseHangfireDashboard(
            pathMatch: "/hangfire",
            options: new DashboardOptions()
            {
                DashboardTitle = "Tuxfamily Scraper",
                Authorization = new[]
                {
                    new HangfireCustomBasicAuthenticationFilter
                    {
                        User = "user",
                        Pass = "password"
                    }
                }
            });
        
        RecurringJob.AddOrUpdate(
            recurringJobId: "tuxfamilyScrape",
            methodCall: () => app.Services.GetRequiredService<IScraperService>().ScrapeSite(),
            cronExpression: Cron.Hourly
        );
    }
}