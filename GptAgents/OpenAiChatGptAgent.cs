using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Runtime.Serialization;
using ServiceStack;
using ServiceStack.Logging;
using SharpToken;

namespace GptAgents;

public class OpenAiChatGptAgent : ILanguageModelAgent
{
    private readonly string _model;
    public GptAgentData Data { get; set; }
    
    public string ApiKey { get; set; }

    public int TokenLimit { get; set; }
    
    protected const string BaseUrl = "https://api.openai.com/v1/chat/completions";

    private ILog? _logger;

    public OpenAiChatGptAgent(string apiKey, GptAgentData data, string model = "gpt-3.5-turbo", int tokenLimit = 4000)
    {
        _model = model;
        ApiKey = apiKey;
        Data = data;
        TokenLimit = tokenLimit;
        InitLogger();
    }

    private void InitLogger()
    {
        if (LogManager.LogFactory == null || !(LogManager.LogFactory is NullLogFactory))
            LogManager.LogFactory = new ConsoleLogFactory();
        _logger = LogManager.GetLogger(typeof(OpenAiChatGptAgent));
    }


    public async Task<AgentChatResponse> ChatAsync(List<AgentTask> tasks, List<ChatMessage> history,
        Dictionary<string, string> memories, string? userPrompt = null)
    {
        using HttpClient client = new HttpClient();
        userPrompt += "\nDetermine which next command to use to complete tasks. You *must* respond in the JSON format specified above:";
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ApiKey);
        
        // Construct main prompt
        var sysPrompt= ConstructFullPrompt(tasks);
        
        var currentTokensUsed = GetTokenCount(userPrompt);
        currentTokensUsed += GetTokenCount(sysPrompt);
        var permMemoryPrompt = $"Permanent memory: \n{memories.Select(x => x.Key + ": " + x.Value).Join("\n")}\n\n";
        currentTokensUsed += GetTokenCount(permMemoryPrompt);

        var taskPrompt = "";
        taskPrompt +=$"\n\nUSER TASKS:\n\n";
        for (var index = 0; index < tasks.Count; index++)
        {
            var task = tasks[index];
            taskPrompt += $"Task {task.TaskRef}: \"\"\"{task.Prompt}\"\"\"\n" +
                          $"Success Criteria: {task.SuccessCriteria}\n\n";
        }
        currentTokensUsed += GetTokenCount(taskPrompt);
        var messages = new List<ChatMessage>
        {
            new()
            {
                Role = "system",
                Content = sysPrompt
            },
            new()
            {
                Role = "system",
                Content = permMemoryPrompt
            },
            new ()
            {
                Role = "user",
                Content = taskPrompt
            }
        };
        

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
                messages.Add(nextMsgHistoryToAdd);
                currentTokensUsed += GetTokenCount(nextMsgHistoryToAdd.Content);
                nextMsgHistoryToAddIndex++;
            }
        }

        messages.Add(new ChatMessage
        {
            Role = "user",
            Content = userPrompt
        });

        AgentChatResponse data;
        var requestData = new
        {
            model = _model,
            messages = messages.Select(x => new
            {
                x.Role,
                x.Content
            }),
            max_tokens = 500,
            temperature = 0.0
        };
        
        var response = await client.PostAsJsonAsync(BaseUrl, requestData);
        var responseString = await response.Content.ReadAsStringAsync();


        try
        {
            if (response.IsSuccessStatusCode == false)
                throw new Exception($"Error in chat: {responseString}");
            _logger.Debug($"Chat response: {responseString}");
            //trim non-json response
            var result = responseString.FromJson<ChatCompletionResponse>();
            var content = result.Choices[0].Message.Content;
            data = content.FromJson<AgentChatResponse>();
        }
        catch (Exception e)
        {
            _logger.Error($"Error in chat: {responseString}", e);
            throw;
        }
        history.Add(new ChatMessage
        {
            Role = "user",
            Content = userPrompt
        });
        history.Add(new ChatMessage
        {
            Role = "assistant",
            Content = data.ToJson()
        });
        return data;
    }
    
    /// <summary>
    /// Placeholder calculation for the number of tokens in a string.
    /// Conservative estimate to try and avoid hitting the token limit.
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public virtual int GetTokenCount(string text)
    {
        var encoding = GptEncoding.GetEncoding("cl100k_base");
        return encoding.Encode(text).Count;
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

public class ChatCompletionResponse
{
    public string Id { get; set; }
    public string Object { get; set; }
    public int Created { get; set; }
    public string Model { get; set; }
    public Usage Usage { get; set; }
    public List<ChatChoice> Choices { get; set; }
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
}

public interface ILanguageModelAgent
{
    Task<AgentChatResponse> ChatAsync(List<AgentTask> tasks, List<ChatMessage> history,
        Dictionary<string, string> memories, string? userPrompt = null);
    
    GptAgentData Data { get; }
}