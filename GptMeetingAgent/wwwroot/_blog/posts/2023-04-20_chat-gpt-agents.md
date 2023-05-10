---
title: Leveraging ChatGPT Agents with ServiceStack
summary: A walkthrough of how to enable ChatGPT Agents with access to your own ServiceStack APIs
tags: servicestack, gpt, chatbot, ai
splash: https://images.unsplash.com/photo-1533090161767-e6ffed986c88?crop=entropy&fit=crop&h=1000&w=2000
draft: true
author: Darren Reid
---

<iframe class="video-hd" src="https://www.youtube.com/embed/7vChIGHWPuI" frameborder="0" allow="autoplay; encrypted-media" allowfullscreen></iframe>

## Introduction

We've been working on a proof of concept project to enable [ChatGPT](https://openai.com) to solve tasks using your own ServiceStack APIs.
This is done through various prompting techniques which allows the Agent to reason about the context of the conversation and make decisions to reach for different 'tools' (your APIs) when trying to achieve a specific goal.

Those tools are your own ServiceStack APIs which can be configured through a plugin in the GptMeetingAgent project.

```csharp
var gptAgentFeature = new GptAgentFeature();

gptAgentFeature.RegisterAgent(new GptAgentData
    {
        Name = "BookingAgent",
        PromptBase = File.ReadAllText(
            $"{Path.Combine("Prompts", "BasePromptExample.txt")}"
        ),
        Role = "An AI that makes meeting bookings between staff."
    },
    agentFactory: data => new OpenAiChatGptAgent(chatGptApiKey,data),
    includeApis: new List<string>
    {
        Tags.Teams,
        Tags.Calendar,
    });
```

The above configuration registers a new Agent with the name `BookingAgent` and a description of what it does as the `Role`.

It also is configured to include the APIs with the `Tags.Teams` and `Tags.Calendar` tags, which are the APIs that the Agent will use to achieve its goals.
Only these APIs will be exposed to the Agent, and the Agent will only be able to use them in the context of the prompt that is provided to it.

The Agent will also use the `Description` of the API to assist the Agent in understanding how and when to use the API.

```csharp
[Tag("Teams"), Description("Search for users by name")]
public class SearchUsers : IReturn<SearchUsersResponse>
{
    public string Name { get; set; }
}
```

## So How Does This Work?

This proof of concept uses several different technologies and techniques to achieve this functionality. A lot of which are quite new, and not nearly as well known as common web development techniques.
As such, we will go through each of the technologies and techniques used to achieve this functionality as an introduction to using AI Agents in your own projects.

### OpenAI ChatGPT and Large Language Models

ChatGPT by OpenAI is a chat product that uses large language models that are trained on a large corpus of text from the internet. 
The `GPT` in ChatGPT stands for `Generative Pre-trained Transformer` which is a type of neural network architecture that is used to train the model.

[![](/img/chatgpt-example/Slide10_1080p.png)](https://arxiv.org/abs/1706.03762)

These Large Language Models are designed to predict the next 'token' or part of a word in a sentence, given the previous tokens.

[![](/img/chatgpt-example/Slide11_1080p.png)](https://learn.microsoft.com/en-us/semantic-kernel/concepts-ai/tokens)

This text is called the `prompt` and is used to 'prime' the model with some context to generate the next token.

The use of the Transformer architecture has shown to be very effective at learning the structure of language as well as the semantic meaning of words and sentences.

As these models are scaled up, they are able to learn more and more complex patterns in language, and are able to generate more and more complex sentences.
Some state of the art models have been able to show emergent capabilities of reasoning and problem solving, which is what we are leveraging in this proof of concept.

### Chain-of-Thought

One technique that has been discovered when using these Large Language Models is the ability to prompt in a way that improves the ability of the model to perform complex reasoning.
This is called [Chain-of-Thought](https://arxiv.org/abs/2201.11903) and is a technique asks the model to output its 'thoughts' about a task that it is trying to achieve, and then feeding those thoughts back into the model in the next prompt.

![](/img/chatgpt-example/Slide13_1080p.png)

We can get the model to do this with a surprisingly simple prompt.

```text
Ensure to reason about the tasks in a step by step manner, 
and provide details about your plan, reasoning, criticism, 
and reflection about your steps in the thoughts output.
```

We specifically ask the model to output details about its `plan`, `reasoning`, and `criticism` as they are a part of the final output that we want to generate.

![](/img/chatgpt-example/Slide12_1080p.png)

This enables the model to break down tasks into smaller steps, and then reason about how to achieve those steps by giving it context of the previous thoughts that it has generated.
This reinforces why previous decisions were made, which in turn, influences future decisions.

### Agents

We can then combine the reasoning capabilities of the model with the ability to take action using 'tools' to achieve a goal.

Giving the model agency to use external tools and APIs creates the concept of an `Agent` that can be used to solve a variety of tasks.

We give the Agent access to these tools by describing them in the prompt. Using the type information about your Request DTOs, and the `[Description]` attribute, we can describe the tools that the Agent has access to.
In the prompt, we call these `Service Commands`.

```
[Tag(Tags.Teams)]
[Description("Search for users by first or last name and get back their IDs and emails")]
public class SearchUsers : IReturn<SearchUsersResponse>
{
    public string Name { get; set; }
}
```

This turns into a Service Command with the following text in the prompt.

```text
1. Search for users by first or last name and get 
back their IDs and emails: "SearchUsers", definition: 
type SearchUsers = { name: string }
```

The Service Commands each have the Request DTO name as well as the description to help the Agent understand what the tool does and when to use it.
It also includes a TypeScript definition of the Request DTO so that the Agent can generate the correct request structure when it needs to use the tool, and populate with data from its own context.

### In Context Learning

So now our Agents have access to tools, can reason about problems, and provide feedback about its reasoning, but we still need it to use our own data.

The prompt technique of `In Context Learning` allows us to provide the Agent with data returned from the tools that it uses, and then use that data in the next prompt.

For example, if we want to book a meeting with a colleague, the model can use the `SearchUsers` tool to find the email address of the person we want to book a meeting with.

```json
{
  "commandName": "SearchUsers",
  "commandResponse": [{
      "displayName": "Lynne Schoen",
      "email": "Lynne_Schoen9@gmail.com",
      "id": 6
    }
  ]
}
```

We can then feed back the command and the response into the next prompt, which will allow the model to use the email address of the person we want to book a meeting with.

Another API that is available to the Agent is the `GetUserSchedule` API to check the availability of the person we want to book a meeting with. 
This API requires a `userId`, which the Agent now has from the `SearchUsers` API. 

```
2. Get a user's schedule: "GetUserSchedule", 
definition: type GetUserSchedule = { userId: number, schedules: string[], 
startTime: string, endTime: string, availabilityViewInterval: number }
```

### Output Format

Each time we prompt the Agent, we get back a consistent JSON structure with a `command` and `thoughts` object.

```json
{
    "command": {
        "name": "GetUserSchedule",
        "body": {
            "userId": 6,
            "schedules": ["primary"],
            "startTime": "2023-05-09T00:00",
            "endTime": "2023-05-16T00:00",
            "availabilityViewInterval": 60
        }
    },
    "thoughts": {
        "text": "Checking Lynne's schedule for availability on Tuesday ...",
        "reasoning": "Before creating a meeting, it is important to check ...",
        "plan": "- Use the GetUserSchedule command to retrieve Lynne's calendar ...",
        "criticism": "I may need to account for time zone differences ...",
        "speak": "I am now checking Lynne's schedule for availability..."
    }
}
```

Near the end of each interaction with the model, we specify the response format using the following prompt.

![](/img/chatgpt-example/Slide17.PNG)

The `command` object contains the name of the command that was used, as well as the body of the command that was generated from the Agent's context.
The `thoughts` object contains the text that was generated by the model, as well as the `reasoning`, `plan`, and `criticism` that was generated by the model.

The `body` of the command is a TypeScript definition of the Request DTO that was used, and can be used to generate the correct structure of the command when it is used again.

### Putting it all together

We can now combine all of these concepts to create an Agent that can book a meeting with a colleague.

![](/img/chatgpt-example/AgentFlow.drawio.svg)

In the GptMeetingAgent proof of concept, the browser client is the one taking action on behalf of our agent, making requests to your APIs and returning the response to the Agent.

We can use the `[ConfirmationRequired]` attribute to put in additional guard rails to prevent the Agent from taking action on sensitive APIs.

In the GptMeetingAgent project, this is used to prevent the Agent from booking a meeting without confirmation from the user.
Instead, we use the `Submit` of an `AutoForm` to confirm the booking.


## The GptMeetingAgent Project

The example we have built is a simple API that can search for users, list their schedules, and book meetings between users.

A lot of this example is actually generic and being driven by the Agent and the specific APIs in the example.

This can be demonstrated by introducing a new `ListMeetingRooms` API that we add to the APIs the Agent can use.

```csharp
[Tag(Tags.Teams), Description("Get a list of meeting rooms and their resources.")]
public class ListMeetingRooms : QueryDb<MeetingRoom> { }
```

This AutoQuery service lists meeting rooms along with their related resources like `Projector`,`SmartBoard`, and `Whiteboard`.

```csharp
public enum MeetingRoomResource
{
    SmartBoard,
    Projector,
    Whiteboard,
    VideoConference,
    SpeakerPhone
}
```

We can then extend the `CreateCalendarEvent` API that books the meeting to reference a meeting room by ID.

```csharp
[Tag(Tags.Calendar), Description("Create a meeting")]
[ConfirmationRequired("Are you sure you want to create a meeting?")]
public class CreateCalendarEvent : IReturn<CreateCalendarEventResponse>
{
    public string Subject { get; set; }
    public string Body { get; set; }
    [Input(Type = "datetime-local")]
    public DateTime Start { get; set; }
    [Input(Type = "datetime-local")]
    public DateTime End { get; set; }
    public string AttendeeEmail { get; set; }
    // Newly Added
    public int? MeetingRoomId { get; set; }
}
```

Restarting the application, our injected Service Commands now reflect both the new `ListMeetingRooms` API and the updated `CreateCalendarEvent` API in the Agent prompt.

Asking the Agent to "Ensure the meeting room has a projector" will now result in the behavior of our Agent using our new API, finding the request resource, and passing the correct `MeetingRoomId` when booking a meeting.

Both the Agent and the UI reflect this change in behavior. Our `AutoForms` for both APIs are displayed, and we didn't need to make any other changes other than modifying our APIs to support the new behavior.

You can checkout our [live demo of this example here](https://gptmeetings.netcore.io/).

![](/img/chatgpt-example/Slide16.PNG)

## Request for Feedback

We are [releasing this project]() as a proof of concept to get feedback from the community on how they would like to use this technology in their own projects.
We plan on continuing to develop examples and documentation for a generic pattern to make this type of development easier to integrate by building a plugin similar to that in the GptMeetingAgent project.

We would love to hear your feedback on this project, and how you would like to use it in your own projects.
This will help us to prioritize and focus on the important features of the plugin and the documentation.

<style>
.video-hd { width: 640px; height: 360px }
</style>