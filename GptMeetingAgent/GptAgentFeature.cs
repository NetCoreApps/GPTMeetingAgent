using GptAgents;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Orchestration;
using ServiceStack.DataAnnotations;
using ServiceStack.NativeTypes.TypeScript;

namespace GptMeetingAgent;

public class GptAgentFeature : IPlugin, IPostInitPlugin
{
    private readonly IKernel _kernel;
    public Dictionary<string, string> AgentServiceCommands { get; set; } = new();
    
    private Dictionary<string, List<string>> AgentServiceCommandTags { get; set; } = new();

    public GptAgentFeature(IKernel kernel)
    {
        _kernel = kernel;
    }
    
    public void Register(IAppHost appHost)
    {
        appHost.RegisterService<ChatGptAgentService>();
    }

    public void AfterPluginsLoaded(IAppHost appHost)
    {
        var metadata = appHost.Metadata;
        var _logger = appHost.Resolve<ILoggerFactory>().CreateLogger(typeof(GptAgentFeature));
        foreach (var agentName in AgentFactories.Keys)
        {
            // Each agent has its own set of accessible services
            var tags = AgentServiceCommandTags[agentName];
            var agentRequestDtos = new Dictionary<string,Type>();
            foreach (var tag in tags)
            {
                var operations = metadata.Operations
                    .Where(x => x.Tags != null && x.Tags.Any(y => y == tag))
                    .ToList();

                foreach (var operation in operations)
                {
                    agentRequestDtos[operation.Name] = operation.RequestType;
                }
            }

            var commandLines = new List<string>();

            var count = 1;
            foreach(var agentService in agentRequestDtos)
            {
                var requestType = agentService.Value;
                var descriptionAttribute = requestType.FirstAttribute<DescriptionAttribute>();
                if (descriptionAttribute == null)
                {
                    _logger.LogWarning("No description attribute found for {0}", requestType.Name);
                    continue;
                }
                var desc = descriptionAttribute.Description;
                var operation = metadata.GetOperation(requestType);
                if (operation != null)
                {
                    var metadataTypes = metadata.GetMetadataTypesForOperation(null, operation);
                    var reqMetadata = metadataTypes[0];
                    var reqProperties = reqMetadata.Properties;
                    if (reqProperties != null)
                    {
                        var reqPropertyNames = reqProperties.Select(x => new {x.Name, x.Type, MetadataPropertyType = x}).ToList();
                        commandLines.Add(
                            $"{count}. {desc}: \"{requestType.Name}\", " +
                            $"definition: type {requestType.Name} = " +
                            $"{{ {reqPropertyNames.Select(x => $"{ResolveTypeScriptPropertyName(x.Name, x.MetadataPropertyType)}: {ResolveTypeScriptType(x.MetadataPropertyType)}").Join(", ")} }}");    
                    }
                    else
                    {
                        commandLines.Add($"{count}. {desc}: \"{requestType.Name}\", " +
                                         $"definition: type {requestType.Name} = {{}}");
                    }
                }

                count++;
            }
            AgentServiceCommands.Add(agentName, commandLines.Join("\n") + "\n");
        }
    }
    
    private string ResolveTypeScriptPropertyName(string name, MetadataPropertyType type)
    {
        if(!type.PropertyType.IsNullableType())
            return name.ToCamelCase();
        return name.ToCamelCase() + "?";
    }

    private string ResolveTypeScriptType(MetadataPropertyType type)
    {
        if (TypeScriptGenerator.ArrayTypes.Contains(type.Type))
        {
            if (type.PropertyType.IsGenericType)
            {
                var genericType = type.PropertyType.GetGenericArguments()[0];
                var genericTypeScriptType = TypeScriptGenerator.TypeAliases[genericType.Name];
                return $"{genericTypeScriptType}[]";
            }

            return "[]";
        }
        if (type.PropertyType.IsGenericType)
        {
            var genericType = type.PropertyType.GetGenericArguments()[0];
            var genericTypeScriptType = TypeScriptGenerator.TypeAliases[genericType.Name];
            return $"{genericTypeScriptType}";
        }
        if(TypeScriptGenerator.TypeAliases.TryGetValue(type.Type, out var scriptType))
            return scriptType;

        return type.Type;
    }

    public Dictionary<string, Func<GptAgentData, ILanguageModelAgent>> AgentFactories = new();

    public async Task<ILanguageModelAgent> CreateAgentAsync(string agentName)
    {
        var factory = AgentFactories[agentName];
        var data = AgentDataMappings[agentName];
        var agent = factory(data);
        // Replace Service Commands
        var prompt = agent.Data.PromptBase;
        var context = new SKContext();
        context["serviceCommands"] = AgentServiceCommands[agentName];
        agent.Data.PromptBase = await _kernel.PromptTemplateEngine.RenderAsync(prompt,context);
        return agent;
    }
    
    public ILanguageModelAgent CreateAgent(GptAgentData agentData)
    {
        if (!AgentFactories.ContainsKey(agentData.Name))
            throw new ArgumentException($"AgentFactory for {agentData.Name} not registered");

        return AgentFactories[agentData.Name](agentData);
    }

    /// <summary>
    /// Register an agent with specific Service Tags.
    /// This will expose the Agent to have access to call those services when performing
    /// chain of thought prompts.
    /// TODO - GptAgentData PromptBase should have a fallback default
    /// </summary>
    /// <param name="gptAgentData"></param>
    /// <param name="includeApis"></param>
    /// <param name="agentFactory"></param>
    /// <exception cref="ArgumentException"></exception>
    public GptAgentFeature RegisterAgent(GptAgentData gptAgentData, List<string> includeApis, Func<GptAgentData, ILanguageModelAgent> agentFactory)
    {
        if(gptAgentData.Name.IsNullOrEmpty())
            throw new Exception("Agent must have a name");
        if (AgentFactories.ContainsKey(gptAgentData.Name))
            throw new ArgumentException($"AgentFactory for {gptAgentData.Name} already registered");

        AgentFactories[gptAgentData.Name] = agentFactory;
        
        AgentServiceCommandTags[gptAgentData.Name] = includeApis;
        
        AgentDataMappings[gptAgentData.Name] = gptAgentData;
        return this;
    }
    
    private Dictionary<string, GptAgentData> AgentDataMappings { get; set; } = new();

    private Dictionary<string, Func<object,object,object>> AgentCommandResultFilters { get; set; } = new();

    public GptAgentFeature RegisterCommandTransform<T>(Func<object,object,object> filter)
    {
        var commandName = typeof(T).Name;
        AgentCommandResultFilters[commandName] = filter;
        return this;
    }
    
    public object TransformCommandResponse(AgentCommand? command, object response)
    {
        if (command != null && AgentCommandResultFilters.TryGetValue(command.Name, out var filter))
            return filter(response,command.Body);
        return response;
    }
}

public static class ChatAgentFeatureExtensions
{
    public static string GetAgentThoughtText(this AgentThoughts thoughts)
    {
        if (!thoughts.Speak.IsNullOrEmpty())
            return thoughts.Speak;
        if (!thoughts.Text.IsNullOrEmpty())
            return thoughts.Text;
        if (!thoughts.Reasoning.IsNullOrEmpty())
            return thoughts.Reasoning;
        if (!thoughts.Plan.IsNullOrEmpty())
            return thoughts.Plan;
        return "";
    }
}



