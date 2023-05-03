---
title: Leveraging ChatGPT Agents with ServiceStack
summary: A walkthrough of how to enable ChatGPT Agents with access to your own ServiceStack APIs
tags: servicestack, gpt, chatbot, ai
splash: https://images.unsplash.com/photo-1533090161767-e6ffed986c88?crop=entropy&fit=crop&h=1000&w=2000
draft: true
author: Darren Reid
---

## Introduction

We've been working on a proof of concept project to enable [ChatGPT](https://openai.com) to solve tasks using your own ServiceStack APIs.
This is done through a prompting technique called Chain-of-Thought which allows the Agent to reason about the context of the conversation and make decisions to reach for different tools when trying to achieve a goal.

Those tools are your own ServiceStack APIs which can be configured through a ServiceStack plugin.

```csharp
var chatGptFeature = new ChatAgentFeature();

chatGptFeature.RegisterAgent(new GptAgentData
    {
        Name = "BookingAgent",
        PromptBase = File.ReadAllText(
            $"{Path.Combine("Prompts", "BasePromptExample.txt")}"
        ),
        Role = "An AI that makes meeting bookings between staff."
    },
    agentFactory: data => new OpenAiChatGptAgent(chatGptApiKey,data),
    serviceTags: new List<string>
    {
        "Teams",
        "Calendar",
    });
```

The above configuration registers a new Agent with the name `BookingAgent` and a description of what it does.

It also registers the Agent with the `Teams` and `Calendar` tags which are used to filter the APIs that are exposed to the Agent.

```csharp
[Tag("Teams"), Description("Search for users by name")]
public class SearchUsers : IReturn<SearchUsersResponse>
{
    public string Name { get; set; }
}
```

The Agent then uses the metadata of your ServiceStack APIs to build a vocabulary of how to use them to achieve user provided goals like booking a meeting.

> I want to have a meeting with Lynne regarding Marketing Plan on Thursday the 20th sometime in the morning.

The AI Agent will then use your APIs to achieve the goal of booking a meeting. 
In our example, the Agents search for users by name, search for available meeting times, and then book the meeting, all via your own APIs.

## So How Does This Work?

One of the most interesting patterns to come out of recent AI developments is the ability to build AI Agents using [Chain-of-Thought](https://arxiv.org/abs/2201.11903) prompting techniques.
This has shown to significantly improve the ability of large language models to perform what looks like complex reasoning.

This is what projects like [AutoGPT](https://github.com/Significant-Gravitas/Auto-GPT) and [LangChain](https://github.com/hwchase17/langchain) have shown can be used to expose Agents to additional tools and functionality. 
This is similar functionality to the announced [Plugin Support for GPT-4 from OpenAI](https://openai.com/blog/chatgpt-plugins).
They achieve this through exposing an OpenAPI specification to OpenAI API via a registered application, and then OpenAI systems will reach out to those specific APIs when it thinks it needs to.
And while this feature is impressive, it is limited to applications that are registered as an OpenAI Plugin, where as the Chain-of-Thought approach allows you to expose your own APIs to any Agent built on top of a capable large language model.

The language model is seemingly able to reason about the context of the conversation and make decisions to reach for different tools when trying to achieve a goal.
This 'context' is in the text prompt that is provided to the Agent, also known as the `context window`.
This window is a relatively tiny amount of information that the Agent can use along with its own knowledge to make decisions.
It doesn't know about your internal APIs, or how to use them, but it can be shown through the text prompt.

In a lot of the popular examples like AutoGPT, this is done through a mapping of methods (generally Python) to the Agent's vocabulary, or (if you're game) allowing the Agent to generate and run its own code.
The text prompt needs to provide instructions on how to utilize these tools by showing the Agent the name and argument structure of how to use them,
along with a small description of what they do. The human user then provides a task or set of tasks for the Agent to achieve and the Agent is able to reach for these tools in the pursuit of completing the tasks.

We have created a proof of concept example of how to enable this functionality with your own ServiceStack APIs, while using OpenAI GPT-3.5+ as the Agent's language model.
And we use the well structured metadata of your ServiceStack APIs to provide the Agent with a consistent vocabulary of how to use your APIs so you don't need to manage that integration yourself.

Auto-GPT (at the time of writing) does this by generating commands that look like the following.

```
1. CommandName: A description of command, args: (arg1: int, arg2: str) -> str"
```

You can do something similar with .NET/C#, but in the context of web service APIs, we can use the metadata of your ServiceStack APIs to provide the Agent with a consistent vocabulary of how to use your APIs.
In the example, we have done this by following a form of TypeScript type definitions, which are also used by the ServiceStack TypeScript generator.

```
1. Search for users by name: "SearchUsers", definition: type SearchUsers = { name: string }
```

And instead of the server looping through the chain-of-thought commands on the server on behalf of the Agent, we send the Agent command options back to the web client to call those ServiceStack services, and pass the result back to Agent.

![flow of data diagram](/img/chatgpt-example/AgentFlow.drawio.svg)


## The GptMeetingAgent Project

The example we have built is a simple API that can search for users, list their schedules, and book meetings between users.

For example, lets have another look at the prompt that the Agent uses to book a meeting:

> I want to have a meeting with Lynne regarding Marketing Plan on Thursday the 20th sometime in the morning.

This seemly innocuous prompt is full of fuzzy information that the Agent needs to use and make decisions on.
It needs to use the tools at its disposal to fetch the information it needs to book the meeting.

It needs to:
 - Search for a user named Lynne
 - Check their schedule using their userID, and the preferred date and time
 - Bring together information from the prompt and results of the search and schedule to book a meeting

The Agent has some other commands at its disposal to help like adding what it thinks is important information into memory which is fed back into the prompt for the next command.

This helps to avoid problems with the Agent forgetting information due to the limited context window of the prompt.