using GptAgents;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.AI.ChatCompletion;
using Microsoft.SemanticKernel.Planning;
using Microsoft.SemanticKernel.Planning.Sequential;
using ServiceStack.OrmLite;

namespace GptMeetingAgent;

public class ChatGptAgentService : Service
{
    public IKernel Kernel { get; set; }
    
    public ILoggerFactory LoggerFactory { get; set; }

    public ILogger Logger => LoggerFactory.CreateLogger(typeof(ChatGptAgentService));
    
    /// <summary>
    /// Get a list of all the tasks for a given agent.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public async Task<object> Get(ListTasksForAgent request)
    {
        var agentTasks = await Db.LoadSelectAsync<StoredAgentTask>(
            x => x.StoredAgentDataId == request.AgentId);
        return agentTasks;
    }

    /// <summary>
    /// Get chat history for a task to be displayed for the user.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public async Task<object> Get(ListChatHistoryForTask request)
    {
        var chatHistory = await GetHistoryForUser(request.TaskId);
        return new ListChatHistoryForTaskResponse
        {
            ChatHistory = chatHistory.ToList()
        };
    }

    public async Task<object> Post(CreateStoredAgent request)
    {
        var feature = HostContext.GetPlugin<GptAgentFeature>();
        var agent = await feature.CreateAgentAsync(request.AgentType);
        var agentData = agent.Data.ConvertTo<StoredAgentData>();
        var agentId = await Db.InsertAsync(agentData, selectIdentity: true);
        agentData.Id = (int)agentId;
        return agentData;
    }

    /// <summary>
    /// Update the command history to include the response from the agent. 
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public async Task<object> Post(UpdateAgentCommand request)
    {
        var command = await Db.SingleByIdAsync<StoredAgentCommand>(request.CommandId);
        if(command == null)
            throw HttpError.NotFound("Command not found");

        if (request.CommandResponse != null)
        {
            command.Response = request.CommandResponse;
            await Db.SaveAsync(command);
        }
        
        return new UpdateAgentCommandResponse
        {
            Command = command
        };
    }

    /// <summary>
    /// Filter the message history to only include chat messages that are valid for the GPT agent.
    /// Messages intended to display to the user and include as history for the LLM agent as
    /// given different roles. This follows the OpenAI Chat API where the roles are:
    /// "system", "user", "assistant".
    /// UI roles are "Human", "Agent".
    /// They are case sensitive.
    /// Non-OpenAI LLMs can still use "system", "user", "assistant" as roles, but how that is
    /// passed to the LLM will depend on the implementation.
    /// TODO: This is currently tightly coupled to OpenAiChatGptAgent.
    /// </summary>
    /// <param name="taskId"></param>
    /// <returns></returns>
    private async Task<List<ChatMessage>> GetHistoryForGpt(int taskId)
    {
        var query = Db.From<StoredChatMessage>()
            .Where(x => x.StoredAgentTaskId == taskId)
            .Where(x => Sql.In(x.Role, OpenAiChatGptAgent.ValidChatGptRoles))
            .OrderBy(x => x.Id);
        return (await Db.SelectAsync(query)).ToList().Select(x => x as ChatMessage).ToList();
    }

    private readonly List<string> _userChatMessageRoles = new()
    {
        "Human",
        "Agent",
    };
    
    /// <summary>
    /// Get the chat history to display to the User.
    /// Additional chat messages are added to the history to provide context for the user.
    /// Messages intended to display to the user and include as history for the LLM agent prompt, are
    /// given different roles. This follows the OpenAI Chat API where the roles are:
    /// "system", "user", "assistant".
    /// UI roles are "Human", "Agent".
    /// They are case sensitive.
    /// Non-OpenAI LLMs can still use "system", "user", "agent" as roles, but how that is
    /// passed to the LLM will depend on the implementation.
    /// </summary>
    /// <param name="taskId"></param>
    /// <returns></returns>
    private async Task<List<StoredChatMessage>> GetHistoryForUser(int taskId)
    {
        var query = Db.From<StoredChatMessage>()
            .Where(x => x.StoredAgentTaskId == taskId)
            .Where(x => Sql.In(x.Role, _userChatMessageRoles))
            .OrderBy(x => x.Id);
        return (await Db.LoadSelectAsync(query)).ToList();
    }

    /// <summary>
    /// Continue working on an existing task.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public async Task<object> Post(ContinueGptAgentTask request)
    {
        var agentTask = await Db.SingleByIdAsync<StoredAgentTask>(request.TaskId);
        if(agentTask == null)
            throw HttpError.NotFound("Task not found");
        if (agentTask.Completed)
            return new ContinueGptAgentTaskResponse
            {
                Task = agentTask
            };
        var agentData = await Db.SingleByIdAsync<StoredAgentData>(agentTask.StoredAgentDataId);
        var memoryKeyPrefix = $"{agentTask.StoredAgentDataId}:{agentTask.TaskRef}";
        var memoryKeys = Cache.GetKeysByPattern($"{memoryKeyPrefix}:*");
        var memory = memoryKeys != null
            ? memoryKeys.ToDictionary(x => x.Replace(memoryKeyPrefix,""), x => Cache.Get<string>(x))
            : new Dictionary<string, string>();

        var feature = HostContext.GetPlugin<GptAgentFeature>();


        if (request.Command?.Name.IsNullOrEmpty() == false && request.CommandResponse != null)
        {
            var filteredCommandResponse = feature.TransformCommandResponse(request.Command, request.CommandResponse);
            var commandResponse = new
            {
                CommandName = request.Command.Name,
                CommandResponse = filteredCommandResponse
            };
            await Db.SaveAsync(new StoredChatMessage
            {
                Role = "system",
                Content = commandResponse.ToJson(config => config.ExcludeTypeInfo = true) ?? string.Empty,
                StoredAgentTaskId = agentTask.Id
            });
            // Save command response to the database.
            request.Command.Response = request.CommandResponse;
            await Db.SaveAsync(request.Command);
        }

        var agent = await feature.CreateAgentAsync(agentData.Name);
        var localHistory = await GetHistoryForGpt(agentTask.Id);
        var chatHistory = await agent.ConstructChat(
            new List<AgentTask> { agentTask },
            localHistory,
            memory,
            request.UserPrompt
        );

        Logger.LogInformation($"ChatHistory: {chatHistory.ToJson()}");
        
        var chatCompletionService = Kernel.GetService<IChatCompletion>();
        var result = await chatCompletionService.GenerateMessageAsync(chatHistory, new ChatRequestSettings
        {
            Temperature = 0.0,
            MaxTokens = 512
        });
        var chatResponse = agent.ParseResponse(result);

        
        var command = await PersistInteraction(request.UserPrompt, agentTask, chatResponse);
        var skipCommand = HandleInternalCommands(chatResponse, agentTask);
        if (chatResponse.Command is { Name: "CompleteTask" })
        {
            agentTask.Completed = true;
            await Db.SaveAsync(agentTask);
        }

        bool hasQuestion = false;
        if (chatResponse.Command is { Name: "AskQuestion" })
        {
            skipCommand = true;
            hasQuestion = true;
        }

        var response = new ContinueGptAgentTaskResponse
        {
            Thoughts = chatResponse.Thoughts,
            Command = skipCommand ? null : command,
            Task = agentTask,
            Question = hasQuestion
        };
        return response;
    }

    /// <summary>
    /// A list of commands that are not intended for action by the client
    /// on behalf of the agent. These commands are handled by the server.
    /// </summary>
    private readonly List<string> _localCommands = new()
    {
        "MemoryAdd",
        "MemoryDel",
        "MemoryReplace",
        "CompleteTask"
    };

    /// <summary>
    /// Start a new task with a new agent.
    /// Specify the agent type or agent id.
    /// An agentType should match one that is registered with the ChatAgentFeature.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public async Task<object> Post(StartGptAgentTask request)
    {
        var feature = HostContext.GetPlugin<GptAgentFeature>();
        ILanguageModelAgent agent;
        
        int agentId;
        if (request.AgentId != null)
        {
            var existingAgentData = await Db.SingleByIdAsync<StoredAgentData>((int)request.AgentId);
            agent = feature.CreateAgent(existingAgentData);
            agentId = existingAgentData.Id;
        }
        else if(request.AgentType != null)
        {
            agent = await feature.CreateAgentAsync(request.AgentType);
            var newAgentData = agent.Data.ConvertTo<StoredAgentData>();
            var id = Db.Insert(newAgentData, selectIdentity: true);
            newAgentData.Id = (int)id;
            agentId = newAgentData.Id;
        }
        else
        {
            throw HttpError.BadRequest("AgentId or AgentType must be provided.");
        }

        var agentData = await Db.SingleByIdAsync<StoredAgentData>(agentId);

        var storedAgentTask = new StoredAgentTask
        {
            Prompt = request.Prompt,
            SuccessCriteria = request.SuccessCriteria,
            TaskRef = Guid.NewGuid().ToString().Substring(0, 6),
            Completed = false,
        };

        // History empty when creating a new agent task.
        // Memory also empty when create a new agent task.
        var history = new List<ChatMessage>();
        var chatHistory = await agent.ConstructChat(
            new List<AgentTask> { storedAgentTask },
            history,
            new()
        );
        
        Logger.LogInformation($"ChatHistory Start: {chatHistory.ToJson()}");

        var chatCompletionService = Kernel.GetService<IChatCompletion>();
        var result = await chatCompletionService.GenerateMessageAsync(chatHistory, new ChatRequestSettings
        {
            Temperature = 0.0,
            MaxTokens = 512
        });
        var chatResponse = agent.ParseResponse(result);
        
        var task = await PersistTask(storedAgentTask, agentData);
        var command = await PersistInteraction(request.Prompt, task, chatResponse);
        var skipCommand = HandleInternalCommands(chatResponse, task);
        if (skipCommand)
        {
            // Store the command response in the chat history
            // So that the agent knows they acted on the command.
            Db.Save(new StoredChatMessage
            {
                Role = "system",
                Content = chatResponse.Thoughts.GetAgentThoughtText(),
                StoredAgentTaskId = task.Id
            });
        }
        
        var hasQuestion = false;
        if (chatResponse.Command is { Name: "AskQuestion" })
        {
            skipCommand = true;
            hasQuestion = true;
        }
        
        return new StartGptAgentTaskResponse
        {
            Thoughts = chatResponse.Thoughts,
            Command = skipCommand ? null : command,
            Task = task,
            Question = hasQuestion
        };
    }

    private async Task<StoredAgentTask> PersistTask(StoredAgentTask storedAgentTask, StoredAgentData agentData)
    {
        storedAgentTask.StoredAgentDataId = agentData.Id;
        var taskId = await Db.InsertAsync(storedAgentTask, selectIdentity: true);
        var task = await Db.SingleByIdAsync<StoredAgentTask>(taskId);
        return task;
    }

    private async Task<StoredAgentCommand?> PersistInteraction(string? prompt, StoredAgentTask task, AgentChatResponse chatResponse)
    {
        if (prompt != null && !prompt.IsEmpty())
        {
            // Insert human message into chat history.
            var humanMessage = new StoredChatMessage
            {
                Role = "Human",
                Content = prompt,
                StoredAgentTaskId = task.Id
            };
            var userMessage = new StoredChatMessage
            {
                Role = "user",
                Content = prompt,
                StoredAgentTaskId = task.Id
            };
            await Db.SaveAsync(humanMessage);
            await Db.SaveAsync(userMessage);
        }
        
        await Db.SaveAsync(new StoredChatMessage
        {
            Role = "assistant",
            Content = chatResponse.Thoughts.ToJson(),
            StoredAgentTaskId = task.Id
        });
        var chatMessage = new StoredChatMessage
        {
            Role = "Agent",
            Content = chatResponse.Thoughts.GetAgentThoughtText(),
            StoredAgentTaskId = task.Id
        };
        var messageId = Db.Insert(chatMessage, selectIdentity: true);
        chatMessage.Id = (int)messageId;

        // Store command for audit/replay purposes.
        StoredAgentCommand? command = null;
        if (chatResponse.Command != null)
        {
            command = new StoredAgentCommand
            {
                StoredChatMessageId = chatMessage.Id,
                Name = chatResponse.Command.Name,
                Body = chatResponse.Command.Body
            };
            Db.Save(command);
        }

        return command;
    }

    /// <summary>
    /// Handle internal commands that are not to be executed by the client on behalf of the agent.
    /// These commands are handled by the server.
    /// This just manually string matches the CommandName against a list of known commands to persist
    /// the values in the Command.Body to the ICacheClient.
    /// This might want to move to dedicated services so there are no differences between `ServiceCommands`
    /// and `Commands`.
    /// </summary>
    /// <param name="chatResponse"></param>
    /// <param name="agentTask"></param>
    /// <returns></returns>
    private bool HandleInternalCommands(AgentChatResponse chatResponse, StoredAgentTask agentTask)
    {
        var skipCommand = false;
        var memoryKeyPrefix = $"{agentTask.StoredAgentDataId}:{agentTask.TaskRef}";
        
        if (chatResponse.Command != null && _localCommands.Contains(chatResponse.Command.Name))
        {
            var c = chatResponse.Command;
            var args = c.Body.ConvertTo<Dictionary<string,string>>();
            if (chatResponse.Command is { Name: "MemoryAdd" })
            {
                Cache.Add($"{memoryKeyPrefix}:{args["key"].Trim()}", 
                    new
                    {
                        value = args["value"].Trim(), 
                        description = args.TryGetValue("description", out var arg) ? arg.Trim() : ""
                    }.ToJson(config => config.ExcludeTypeInfo = true));
            }
            if (chatResponse.Command is { Name: "MemoryDel" })
            {
                Cache.Remove($"{memoryKeyPrefix}:{args["key"].Trim()}");
            }
            if (chatResponse.Command is { Name: "MemoryReplace" })
            {
                Cache.Replace($"{memoryKeyPrefix}:{args["key"].Trim()}", new
                {
                    value = args["value"].Trim(), 
                    description = args.TryGetValue("description", out var arg) ? arg.Trim() : ""
                }.ToJson(config => config.ExcludeTypeInfo = true));
            }

            skipCommand = true;
        }

        return skipCommand;
    }
}