﻿using GptAgents;
using GptMeetingAgent.ServiceModel;
using Microsoft.Extensions.Azure;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.AI.OpenAI.TextEmbedding;
using Microsoft.SemanticKernel.CoreSkills;
using Microsoft.SemanticKernel.Memory;
using Microsoft.SemanticKernel.SemanticFunctions;

[assembly: HostingStartup(typeof(GptMeetingAgent.ConfigureGpt))]

namespace GptMeetingAgent;

public class ConfigureGpt : IHostingStartup
{
    public void Configure(IWebHostBuilder builder)
    {
        builder.ConfigureAppHost(beforeConfigure: host =>
        {
            var chatGptApiKey = Environment.GetEnvironmentVariable("CHATGPT_API_KEY");

            var kernel = Kernel.Builder
                .WithMemory(new SemanticTextMemory(
                    new VolatileMemoryStore(),
                    new OpenAITextEmbeddingGeneration("text-embedding-ada-002", chatGptApiKey)))
                .Build();
            kernel.Config.AddOpenAIChatCompletionService("gpt-3.5-turbo", chatGptApiKey);
            kernel.ImportSkill(new TimeSkill(), "time");
            kernel.ImportSkill(new TextMemorySkill());
            kernel.ImportSemanticSkillFromDirectory("Skills", "GptAgent");

            host.Register(kernel);
            
            host.Plugins.Add(new GptAgentFeature().RegisterAgent(new GptAgentData
                {
                    Name = "BookingAgent",
                    Role = @"
An AI that makes meeting bookings between staff, ensuring their schedules do not have conflicting events.
Always make bookings in the future.Ensure the booking is on the requested day.
When checking schedules, check the next 7 days, including today.",
                    PromptBase = File.ReadAllText($"{Path.Combine("Prompts", "BasePromptExample.txt")}")
                },
                agentFactory: agentData => new OpenAiChatGptAgent(chatGptApiKey, agentData),
                includeApis: new()
                {
                    Tags.Teams,
                    Tags.Calendar,
                })
                .RegisterCommandTransform<SearchUsers>((responseDto) =>
                {
                    var results = responseDto.ConvertTo<SearchUsersResponse>();
                    if (results != null)
                    {
                        return results.Value.Select(x => new
                        {
                            x.DisplayName,
                            x.Email,
                            x.Id,
                        }).ToList();
                    }
                    return responseDto;
                })
                .RegisterCommandTransform<GetUserSchedule>((responseDto) =>
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
                })
                .RegisterCommandTransform<CreateCalendarEvent>(responseDto =>
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
                            result.Attendee
                        };
                    }
                    return responseDto;
                }));
        });
    }
}