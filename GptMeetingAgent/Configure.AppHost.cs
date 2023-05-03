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

        var chatGptApiKey = Environment.GetEnvironmentVariable("CHATGPT_API_KEY");
        var chatGptFeature = new ChatAgentFeature();
        
        chatGptFeature.RegisterAgent(new GptAgentData
            {
                Name = "BookingAgent",
                PromptBase = File.ReadAllText($"{Path.Combine("Prompts", "BasePromptExample.txt")}"),
                Role =
                    "An AI that makes meeting bookings between staff, ensuring their schedules do not have conflicting events. " +
                    "Always make bookings in the future." +
                    "Ensure the booking is on the requested day." +
                    "When checking schedules, check the next 7 days, including today."
            },
            agentFactory: data => new OpenAiChatGptAgent(chatGptApiKey,data),
            serviceTags: new List<string>
            {
                "Teams",
                "Calendar",
            });

        chatGptFeature.RegisterCommandFilter<SearchUsers>((responseDto) =>
        {
            var results = responseDto.ConvertTo<SearchUsersResponse>();
            if (results != null)
            {
                return results.Value.Select(x => new
                {
                    x.DisplayName,
                    Mail = x.Email,
                    x.Id,
                }).ToList();
            }

            return responseDto;
        });

        chatGptFeature.RegisterCommandFilter<GetUserSchedule>((responseDto) =>
        {
            var results = responseDto.ConvertTo<GetUserScheduleResponse>();
            if (results != null)
            {
                var scheduleItems = new List<ScheduleItem>();
                results.Value.ForEach(x => scheduleItems.AddRange(x.ScheduleItems));
                var r = scheduleItems.Select(x => new
                {
                    x.Status,
                    x.Subject,
                    x.Location,
                    Start = x.Start.ToString("s"),
                    End = x.End.ToString("s"),
                }).ToList();
                return r;
            }

            return responseDto;
        });

        chatGptFeature.RegisterCommandFilter<CreateCalendarEvent>(responseDto =>
        {
            var result = responseDto.ConvertTo<CreateCalendarEventResponse>();
            if (result != null)
            {
                return new
                {
                    Start = result.Start.ToString("s"),
                    End = result.End.ToString("s"),
                    result.Location,
                    result.Subject,
                    result.Body,
                    Attendees = result.Attendee,
                };
            }

            return responseDto;
        });

        Plugins.Add(chatGptFeature);
    }
}