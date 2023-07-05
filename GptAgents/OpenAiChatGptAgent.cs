using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Runtime.Serialization;
using Azure.AI.OpenAI;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.AI.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AI.OpenAI.AzureSdk;
using Microsoft.SemanticKernel.TemplateEngine.Blocks;
using ServiceStack;
using ServiceStack.Logging;
using SharpToken;

namespace GptAgents;

public class OpenAiChatGptAgent : ILanguageModelAgent
{
    public GptAgentData Data { get; set; }
    
    public int TokenLimit { get; set; }
    
    public GptEncoding Encoding { get; set; } = GptEncoding.GetEncoding("cl100k_base");

    /// <summary>
    /// The integration relies on the response format specified in this prompt.
    /// </summary>
    public string OutputPrompt { get; set; } = @"You must *only respond in JSON* format, as described below.

## RESPONSE FORMAT
{
    ""command"": {
        ""name"": ""command name"",
        ""body"": {}
    },
    ""thoughts"": {
        ""text"": ""thought"",
        ""reasoning"": ""reasoning"",
        ""plan"": ""- short bulleted\n- list that conveys\n- long-term plan"",
        ""criticism"": ""constructive self-criticism"",
        ""speak"": ""thoughts summary to say to user""
    }
}";
    public OpenAiChatGptAgent(GptAgentData data, int tokenLimit = 4000)
    {
        Data = data;
        TokenLimit = tokenLimit;
    }
    
    public AgentChatResponse ParseResponse(string response)
    {
        var content = response;
        var data = content.FromJson<AgentChatResponse>();
        return data;
    }

    public async Task<ChatHistory> ConstructChat(
        List<AgentTask> tasks, 
        List<ChatMessage> history,
        List<AgentCommand> commandHistory,
        Dictionary<string, string> memories, string? userPrompt = null)
    {
        userPrompt += "\nDetermine which Service Command to use to collect the information you need to complete your tasks. You *must* respond in the JSON format specified above:";

        // Construct main prompt
        var sysPrompt= ConstructFullPrompt(tasks);
        
        var currentTokensUsed = GetTokenCount(userPrompt);
        currentTokensUsed += GetTokenCount(sysPrompt);
        var permMemoryPrompt = $"## Permanent memory \n{memories.Select(x => x.Key + ": " + x.Value).Join("\n")}\n\n";
        currentTokensUsed += GetTokenCount(permMemoryPrompt);

        var taskPrompt = "";
        taskPrompt +=$"\n\n## USER TASKS\n\n";
        for (var index = 0; index < tasks.Count; index++)
        {
            var task = tasks[index];
            taskPrompt += $"### Task \"\"\"{task.Prompt}\"\"\"\n" +
                          $"### Success Criteria {task.SuccessCriteria}\n\n";
        }
        currentTokensUsed += GetTokenCount(taskPrompt);
        var result = new ChatHistory();
        result.Messages.Add(new SKChatMessage(new Azure.AI.OpenAI.ChatMessage(ChatRole.System, sysPrompt)));
        result.Messages.Add(new SKChatMessage(new Azure.AI.OpenAI.ChatMessage(ChatRole.System, permMemoryPrompt)));
        result.Messages.Add(new SKChatMessage(new Azure.AI.OpenAI.ChatMessage(ChatRole.User, taskPrompt)));

        // Amount for response
        currentTokensUsed += 500;
        var msgHistory = history.Where(x => ValidChatGptRoles.Contains(x.Role))
            .ToList();

        // Ensure messages are sorted as expected.
        msgHistory = msgHistory.OrderBy(x => x.Created).ToList();
        
        var nextMsgHistoryToAddIndex = 0;
        if (msgHistory.Count > 0)
        {
            while (currentTokensUsed < TokenLimit && msgHistory.Count > nextMsgHistoryToAddIndex)
            {
                var nextMsgHistoryToAdd = msgHistory[nextMsgHistoryToAddIndex];
                result.Messages.Add(new SKChatMessage(new Azure.AI.OpenAI.ChatMessage(GetRole(nextMsgHistoryToAdd.Role), nextMsgHistoryToAdd.Content)));
                currentTokensUsed += GetTokenCount(nextMsgHistoryToAdd.Content);
                nextMsgHistoryToAddIndex++;
            }
        }
        
        result.Messages.Add(new SKChatMessage(new Azure.AI.OpenAI.ChatMessage(ChatRole.System,OutputPrompt)));

        result.Messages.Add(new SKChatMessage(new Azure.AI.OpenAI.ChatMessage(ChatRole.User, userPrompt)));

        return result;
    }

    private ChatRole GetRole(string role)
    {
        return role switch
        {
            "user" => ChatRole.User,
            "system" => ChatRole.System,
            "assistant" => ChatRole.Assistant,
            _ => ChatRole.User
        };
    }

    /// <summary>
    /// Placeholder calculation for the number of tokens in a string.
    /// Conservative estimate to try and avoid hitting the token limit.
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public virtual int GetTokenCount(string text)
    {
        return Encoding.Encode(text).Count;
    }

    public List<string> ValidChatRoles => ValidChatGptRoles;

    public static List<string> ValidChatGptRoles = new()
    {
        "user",
        "assistant",
        "system"
    };
    
    public virtual string GetFormattedTime()
    {
        return DateTime.Now.ToString("HH:mm:ss");
    }

    public virtual string GetFormattedDate()
    {
        return DateTime.Now.ToString("yyyy-MM-dd dddd");
    }
    
    public virtual string ConstructFullPrompt(List<AgentTask> tasks)
    {
        var promptStart =
            @"Your decisions must always be made independently made without seeking user assistance unless absolutely required. Play to your strengths as an LLM and pursue simple strategies.";
        var fullPrompt = $"You are {Data.Name}, {Data.Role}\n{promptStart}";
        fullPrompt += $"\n\nThe Time is currently: {GetFormattedTime()}";
        fullPrompt += $"\nThe Date is currently: {GetFormattedDate()}";
        fullPrompt += "\n\n";

        var promptBase = Data.PromptBase;
        
        fullPrompt += promptBase;
        return fullPrompt;
    }
    
}

public class ChatMessage
{
    public ChatMessage()
    {
        Created = DateTime.UtcNow;
    }
    public string Role { get; set; }
    public string Content { get; set; }

    public DateTime Created { get; set; }
}

public class ChatChoice
{
    public ChatMessage Message { get; set; }
    public string FinishReason { get; set; }
    public int Index { get; set; }
}

[DataContract]
public class Usage
{
    [DataMember(Name = "prompt_tokens")]
    public int PromptTokens { get; set; }
    [DataMember(Name = "completion_tokens")]
    public int CompletionTokens { get; set; }
    [DataMember(Name = "total_tokens")]
    public int TotalTokens { get; set; }
}

public class AgentTask
{
    public string TaskRef { get; set; }
    public string Prompt { get; set; }
    public string SuccessCriteria { get; set; }
    public bool Completed { get; set; }
}

public class AgentThoughts
{
    public string Text { get; set; }
    public string Reasoning { get; set; }
    public string Plan { get; set; }
    public string Criticism { get; set; }
    public string Speak { get; set; }
}

public class AgentChatResponse
{
    public AgentCommand? Command { get; set; }
    public AgentThoughts Thoughts { get; set; }
}

public class AgentCommand
{
    public string Name { get; set; }
    public object Body { get; set; }
}

public class GptAgentData
{
    public string PromptBase { get; set; }
    public string Name { get; set; }
    public string Role { get; set; }
    
    public ConstraintsBlock Constraints { get; set; }
    
}

public class InternalCommandsBlock : Block
{
    public InternalCommandsBlock(string? content, ILogger? log = null) : base(content, log)
    {
    }

    private static string? BuildContent(string? content)
    {
        if (content.IsNullOrEmpty())
            return null;
        return @$"COMMANDS:";
    }

    public override bool IsValid(out string errorMsg)
    {
        throw new NotImplementedException();
    }
}

public class ConstraintsBlock : Block
{
    public ConstraintsBlock(List<string?> constraints, ILogger? log = null) : base(BuildConstraints(constraints), log)
    {
    }
    
    private static string? BuildConstraints(List<string?> constraints)
    {
        if (constraints.IsNullOrEmpty())
            return null;
        return @$"CONSTRAINTS: 
{constraints.Where(x => !x.IsNullOrEmpty()).Select(x => $"- {x}").Join("\n")}";
    }

    public override bool IsValid(out string errorMsg)
    {
        errorMsg = "";
        return true;
    }
}

public interface ILanguageModelAgent
{
    Task<ChatHistory> ConstructChat(List<AgentTask> tasks, List<ChatMessage> history,
        List<AgentCommand> commandHistory,
        Dictionary<string, string> memories, string? userPrompt = null);

    AgentChatResponse ParseResponse(string response);
    
    GptAgentData Data { get; }
}