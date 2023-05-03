using ServiceStack.Data;
using ServiceStack.OrmLite;
using ServiceStack.Text;

[assembly: HostingStartup(typeof(GptMeetingAgent.ConfigureDb))]

namespace GptMeetingAgent;

// Database can be created with "dotnet run --AppTasks=migrate"   
public class ConfigureDb : IHostingStartup
{
    public void Configure(IWebHostBuilder builder) => builder
        .ConfigureServices((context,services) =>
        {
            SqliteDialect.Provider.StringSerializer = new JsonStringSerializer();
            services.AddSingleton<IDbConnectionFactory>(new OrmLiteConnectionFactory(
                context.Configuration.GetConnectionString("DefaultConnection") ?? "App_Data/db.sqlite",
                SqliteDialect.Provider));
        });
}
