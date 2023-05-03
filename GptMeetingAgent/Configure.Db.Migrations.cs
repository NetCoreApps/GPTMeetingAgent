using ServiceStack;
using ServiceStack.Data;
using ServiceStack.OrmLite;
using GptMeetingAgent.Migrations;

[assembly: HostingStartup(typeof(GptMeetingAgent.ConfigureDbMigrations))]

namespace GptMeetingAgent;

// Code-First DB Migrations: https://docs.servicestack.net/ormlite/db-migrations
public class ConfigureDbMigrations : IHostingStartup
{
    public void Configure(IWebHostBuilder builder) => builder
        .ConfigureAppHost(afterAppHostInit:appHost => {
            var migrator = new Migrator(appHost.Resolve<IDbConnectionFactory>(), typeof(Migration1000).Assembly);
            migrator.Timeout = TimeSpan.Zero;
            AppTasks.Register("migrate", _ => migrator.Run());
            AppTasks.Register("migrate.revert", args => migrator.Revert(args[0]));
            AppTasks.Run();
        });
}
