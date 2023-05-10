using System.Globalization;
using Funq;
using GptAgents;
using GptMeetingAgent.ServiceInterface;
using GptMeetingAgent.ServiceModel;
using ServiceStack.DataAnnotations;
using ServiceStack.NativeTypes;
using ServiceStack.OrmLite;
using ServiceStack.Text;
using ServiceStack.Web;

[assembly: HostingStartup(typeof(GptMeetingAgent.AppHost))]

namespace GptMeetingAgent;

public class AppHost : AppHostBase, IHostingStartup
{
    public void Configure(IWebHostBuilder builder) => builder
        .ConfigureServices(services =>
        {
            // Configure ASP.NET Core IOC Dependencies
        });

    public AppHost() : base("GptMeetingAgent", typeof(MyServices).Assembly, typeof(ChatGptAgentService).Assembly)
    {
    }

    public override void Configure(Container container)
    {
        SetConfig(new HostConfig
        {
        });

        JsConfig.AssumeUtc = true;
        JsConfig.DateHandler = DateHandler.ISO8601;
        JsConfig.TextCase = TextCase.CamelCase;

        Plugins.Add(new CorsFeature(new[]
        {
            "http://localhost:5173", //vite dev
        }, allowCredentials: true));
    }
}