using Azure.AI.OpenAI;
using Microsoft.SemanticKernel.AI.TextCompletion;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.AI.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AI.OpenAI.AzureSdk;
using ServiceStack;
using ServiceStack.Text;

namespace GptAgents;


public class OobaboogaChatCompletion : ITextCompletion, IChatCompletion
{
    private readonly HttpClient _httpClient;
    private readonly string _apiUrl;

    public OobaboogaChatCompletion(string apiUrl)
    {
        _httpClient = new HttpClient();
        _apiUrl = apiUrl;
    }

    public async Task<string> CompleteAsync(string text, CompleteRequestSettings requestSettings,
        CancellationToken cancellationToken = new())
    {
        var request = new OobaBoogaRequest()
        {
            Prompt = text,
            Temperature = requestSettings.Temperature,
            TopP = requestSettings.TopP,
            MaxNewTokens = requestSettings.MaxTokens,
            RepetitionPenalty = requestSettings.FrequencyPenalty,
            StoppingStrings = requestSettings.StopSequences.ToArray(),
        };
        
        request.PopulateWithNonDefaultValues(GetDefaults());
        
        using var jsConfig = JsConfig.With(new Config
        {
            TextCase = TextCase.SnakeCase
        });

        var requestJson = request.ToJson();
        var requestContent = new StringContent(requestJson, System.Text.Encoding.UTF8, "application/json");

        HttpResponseMessage response = await _httpClient.PostAsync(_apiUrl, requestContent, cancellationToken);

        response.EnsureSuccessStatusCode();

        var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
        var result = responseJson.FromJson<TextCompletionResponse>();
        if (result.Results.FirstOrDefault(x => !x.Text.StartsWith("{")) != null)
        {
            result.Results[0].Text = result.Results[0].Text.Substring(result.Results[0].Text.IndexOf("{", StringComparison.Ordinal));
        }

        return result?.Results.Select(x => x.Text).Join("\n") ?? string.Empty;
    }
    

    public ChatHistory CreateNewChat(string instructions = "")
    {
        if(!instructions.IsNullOrEmpty())
            return new ChatHistory()
            {
                Messages = { new SKChatMessage(new Azure.AI.OpenAI.ChatMessage(ChatRole.System, instructions)) }
            };
        return new();
    }

    public Task<IReadOnlyList<IChatResult>> GetChatCompletionsAsync(ChatHistory chat, ChatRequestSettings? requestSettings = null,
        CancellationToken cancellationToken = new CancellationToken())
    {
        throw new NotImplementedException();
    }

    public async IAsyncEnumerable<IChatStreamingResult> GetStreamingChatCompletionsAsync(ChatHistory chat, ChatRequestSettings? requestSettings = null,
        CancellationToken cancellationToken = new CancellationToken())
    {
        var prompt = "";
        foreach (var chatMessage in chat.Messages)
        {
            yield return new OobaboogaChatStreamingResult(GetMyRole(chatMessage.Role), chatMessage.Content);
            prompt += $"### {GetRole(chatMessage.Role)} \n{chatMessage.Content}\n";
        }
    }

    public async Task<string> GenerateMessageAsync(ChatHistory chat, ChatRequestSettings? requestSettings = null,
        CancellationToken cancellationToken = new CancellationToken())
    {
        var prompt = "";
        foreach (var chatMessage in chat.Messages)
        {
            prompt += $"### {GetRole(chatMessage.Role)} \n{chatMessage.Content}\n";
        }

        var result = await CompleteAsync(prompt, requestSettings.ConvertTo<CompleteRequestSettings>(),
            cancellationToken);
        return result;
    }

    private OobaBoogaRequest GetDefaults()
    {
        using var jsConfig = JsConfig.With(new Config
        {
            TextCase = TextCase.SnakeCase
        });
        return @"{
            ""max_new_tokens"": 500,
            ""do_sample"": true,
            ""temperature"": 0.1,
            ""top_p"": 1,
            ""typical_p"": 1,
            ""repetition_penalty"": 1.15,
            ""encoder_repetition_penalty"": 1.1,
            ""top_k"": 0,
            ""min_length"": 0,
            ""no_repeat_ngram_size"": 0,
            ""num_beams"": 1,
            ""penalty_alpha"": 0,
            ""length_penalty"": 1,
            ""early_stopping"": true,
            ""seed"": -1,
            ""add_bos_token"": true,
            ""stopping_strings"": [""### User"",""### Assistant"",""### System""],
            ""truncation_length"": 2048,
            ""ban_eos_token"": false,
            ""stream"": false
          }".FromJson<OobaBoogaRequest>();
    }

    private static string GetRole(AuthorRole role)
    {
        return role.Label;
    }

    private static OobaboogaRoles GetMyRole(AuthorRole role)
    {
        switch (role.Label)
        {
            case "user":
                return OobaboogaRoles.User;
            case "assistant":
                return OobaboogaRoles.Assistant;
            case "system":
                return OobaboogaRoles.System;
            default:
                throw new Exception($"Unexpected role label: {role.Label}");
        }
    }

    public async IAsyncEnumerable<string> GenerateMessageStreamAsync(ChatHistory chat, ChatRequestSettings? requestSettings = null,
        CancellationToken cancellationToken = new CancellationToken())
    {
        foreach (var chatMessage in chat.Messages)
        {
            yield return $"### {GetRole(chatMessage.Role)} \n{chatMessage.Content}\n";
        }
    }

    public Task<IReadOnlyList<ITextResult>> GetCompletionsAsync(string text, CompleteRequestSettings requestSettings,
        CancellationToken cancellationToken = new CancellationToken())
    {
        throw new NotImplementedException();
    }

    public IAsyncEnumerable<ITextStreamingResult> GetStreamingCompletionsAsync(string text, CompleteRequestSettings requestSettings,
        CancellationToken cancellationToken = new CancellationToken())
    {
        throw new NotImplementedException();
    }
}

public class OobaBoogaRequest
{
    public string Prompt { get; set; }
    public int? MaxNewTokens { get; set; }
    public bool? DoSample { get; set; }
    public double? Temperature { get; set; }
    public double? TopP { get; set; }
    public double? TypicalP { get; set; }
    public double? RepetitionPenalty { get; set; }
    public double? EncoderRepetitionPenalty { get; set; }
    public double? TopK { get; set; }
    public int? MinLength { get; set; }
    public double? NoRepeatNgramSize { get; set; }
    public int? NumBeams { get; set; }
    public double? PenaltyAlpha { get; set; }
    public double? LengthPenalty { get; set; }
    public bool? EarlyStopping { get; set; }
    public double? Seed { get; set; }
    public bool? AddBosToken { get; set; }
    public string?[] StoppingStrings { get; set; }
    public int? TruncationLength { get; set; }
    public bool? BanEosToken { get; set; }
    public bool? Stream { get; set; }
}

public class TextCompletionResult
{
    public string Text { get; set; }
}

public class TextCompletionResponse
{
    public List<TextCompletionResult> Results { get; set; }
}

public static class TextGenerationExtensions
{
    public static KernelBuilder WithOobaboogaApiChatCompletionService(this KernelBuilder builder, string apiUrl)
    {
        builder.WithAIService("oobabooga", new OobaboogaChatCompletion(apiUrl));
        return builder;
    }
}

public class OobaboogaChatStreamingResult : IChatStreamingResult
{
    private readonly ChatMessageBase _message;
    private readonly OobaboogaRoles _role;
    
    public OobaboogaChatStreamingResult(OobaboogaRoles role, string content)
    {
        this._role = role;
        _message = new OobaboogaChatMessage(role, content);
    }
    
    public Task<ChatMessageBase> GetChatMessageAsync(CancellationToken cancellationToken = new CancellationToken())
    {
        return Task.FromResult(this._message);
    }

    public async IAsyncEnumerable<ChatMessageBase> GetStreamingChatMessageAsync(CancellationToken cancellationToken = new CancellationToken())
    {
        var streamedOutput = this._message.Content.Split(' ');
        foreach (string word in streamedOutput)
        {
            await Task.Delay(100, cancellationToken);
            yield return new OobaboogaChatMessage(this._role, $"{word} ");
        }
    }
}
    
public class OobaboogaChatMessage : ChatMessageBase
{
    public OobaboogaChatMessage(OobaboogaRoles role,string message) : base(new AuthorRole(role.ToString()), message)
    {
    }
}

public class OobaboogaChatHistory : ChatHistory
{
    public void AddMessage(OobaboogaRoles role, string message)
    {
        Messages.Add(new OobaboogaChatMessage(role, message));
    }
}
    
public enum OobaboogaRoles
{
    System,
    User,
    Assistant
}