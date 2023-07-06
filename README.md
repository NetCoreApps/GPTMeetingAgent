# GPT Meeting Agent

This is a proof of concept application showing the use of OpenAI's APIs and the `gpt-3.5-turbo` model to act as a generic Agent to solve problems with your internal APIs.

## GptAgentFeature Plugin

By leveraging techniques like Chain of Thought, In Context Learning and other Prompt Engineering methods, we are able to customize the Agents behavior.
The `GptAgentFeature` plugin enables us to use the `RegisterAgent` method to customize this behavior as well as which of your internal ServiceStack each Agent has access to invoke.

## Use of Semantic Kernel

To make it easier to integrate with existing LLM projects, we have switched to using the [Semantic Kernel](https://github.com/microsoft/semantic-kernel) to handle the API integration.

```csharp
var kernel = Kernel.Builder
    .WithOpenAIChatCompletionService("gpt-3.5-turbo", chatGptApiKey)
    .Build();

host.Register(kernel);
```

```csharp
host.Plugins.Add(new GptAgentFeature(kernel).RegisterAgent(new GptAgentData
    {
        Name = "BookingAgent",
        Role = @"
An AI that makes meeting bookings between staff, ensuring their schedules do not have conflicting events.
Always make bookings in the future.Ensure the booking is on the requested day.
When checking schedules, check the next 7 days, including today.",
        PromptBase = File.ReadAllText($"{Path.Combine("Prompts", "BasePromptExample.txt")}")
    },
    agentFactory: agentData => new OpenAiChatGptAgent(agentData),
    includeApis: new()
    {
        Tags.Teams,
        Tags.Calendar,
    })
```

The `Role` property on the `GptAgentData` primes the Agent to work on a specific task, in this case booking a meeting between two staff members.
This is combined with a generic `PromptBase` that contains instructions for our agent to be able to handle the ServiceStack API integrations.

The `includeApis` is a list of `Tag` names used with your own Request DTOs using the `[Tag]` attribute.
Tags are matched to inject information into the prompt sent to the Large Language Model (LLM) so that it is aware of what tools are available,
as well as a `[Description]` of what each API does. The `[Description]` tag is very important, as it will be the context the Agent uses about when to reach for your API.

## Live Demo

We have a live demo of the [GptMeetingAgent](https://gptmeetings.netcore.io) you can try out.

## Run it yourself

To run this proof of concept, you will need an API key from OpenAI populated in the `CHATGPT_API_KEY` environment variable or populated in the `Configure.Gpt.cs` file.
You will also need to run the `migrate` AppTask to initialize the database using the command `npm run migrate`.

## Feedback

We are looking to develop a highly reusable set of components to leverage this technology and looking for feedback about how developers are using these types of integrations.

As this example shows, we have a generic ServiceStack Plugin that can be used to dynamically create UIs related to the command the Agent decides to use.
We think this combination of UI and Server components creates an interesting new method of development that we are interested exploring more in the future.

Please reach out to us on Discord, GitHub Discussions, Customer Forums or YouTube comments with your use cases, and details about how you want to leverage large language models within your systems.

