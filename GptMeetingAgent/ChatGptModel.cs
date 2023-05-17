using GptAgents;
using ServiceStack.DataAnnotations;

namespace GptMeetingAgent;

[Tag("GptAgents")]
public class ContinueGptAgentTask : IReturn<ContinueGptAgentTaskResponse>
{
    public int TaskId { get; set; }
    public StoredAgentCommand? Command { get; set; }
    public object? CommandResponse { get; set; }
    public string? UserPrompt { get; set; }
}

public class ContinueGptAgentTaskResponse
{
    public StoredAgentTask Task { get; set; }
    public StoredAgentCommand? Command { get; set; }
    public AgentThoughts Thoughts { get; set; }
    public bool Question { get; set; }
}

[Tag("GptAgents")]
public class AnswerGptQuery : IReturn<AnswerGptQueryResponse>
{
    public int TaskId { get; set; }
    public string Prompt { get; set; }
}

public class AnswerGptQueryResponse
{
    public StoredAgentTask Task { get; set; }
    public AgentCommand? Command { get; set; }
    public AgentThoughts Thoughts { get; set; }
}

[Tag("GptAgents")]
public class ListTasksForAgent : IReturn<ListTasksForAgentResponse>

{
    public int AgentId { get; set; }
}

public class ListTasksForAgentResponse
{
    public List<StoredAgentTask> Tasks { get; set; }
}

[Tag("GptAgents")]
public class ListChatHistoryForTask : IReturn<ListChatHistoryForTaskResponse>

{
    public int TaskId { get; set; }
}

public class ListChatHistoryForTaskResponse
{
    public List<StoredChatMessage> ChatHistory { get; set; }
}

[Tag("GptAgents")]
public class StartGptAgentTask : IReturn<StartGptAgentTaskResponse>

{
    public string Prompt { get; set; }
    public string SuccessCriteria { get; set; }
    public int? AgentId { get; set; }
    public string? AgentType { get; set; }
}

public class StartGptAgentTaskResponse
{
    public StoredAgentTask Task { get; set; }
    public AgentThoughts Thoughts { get; set; }
    public StoredAgentCommand? Command { get; set; }
    public bool Question { get; set; }
}

public class StoredAgentTask : AgentTask
{
    [AutoIncrement]
    public int Id { get; set; }

    public int StoredAgentDataId { get; set; }
    
    public string? Name { get; set; }

    [Reference]
    public List<StoredChatMessage> ChatHistory { get; set; }
}

public class StoredAgentData : GptAgentData

{
    [AutoIncrement]
    public int Id { get; set; }
}

public class QueryStoredAgents : QueryDb<StoredAgentData>
{
}

public class CreateStoredAgent : IReturn<StoredAgentData>
{
    public string AgentType { get; set; }
}

public class StoredChatMessage : ChatMessage
{
    [AutoIncrement]
    public int Id { get; set; }
    
    public bool ContentIsJson { get; set; }

    public int StoredAgentTaskId { get; set; }
    
    [Reference]
    public StoredAgentCommand? Command { get; set; }
}

public class StoredAgentCommand : AgentCommand
{
    [AutoIncrement]
    public int Id { get; set; }
    public object Response { get; set; }
    
    public int StoredChatMessageId { get; set; }
}

public class QueryStoredCommands : QueryDb<StoredAgentCommand> { }

public class QueryStoredAgentTasks : QueryDb<StoredAgentTask> { }

public class UpdateAgentCommand : IReturn<UpdateAgentCommandResponse>
{
    public int CommandId { get; set; }
    public object? CommandResponse { get; set; }
}

public class UpdateAgentCommandResponse
{
    public StoredAgentCommand? Command { get; set; }
}

public class UpdateAgentTask : IPatchDb<StoredAgentTask>, IReturn<StoredAgentTask>
{
    public int Id { get; set; }
    public bool Completed { get; set; }
}