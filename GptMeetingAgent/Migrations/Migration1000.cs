using GptAgents;
using GptMeetingAgent.ServiceInterface;
using ServiceStack;
using ServiceStack.DataAnnotations;
using ServiceStack.OrmLite;

namespace GptMeetingAgent.Migrations;

public class Migration1000 : MigrationBase
{
    public class StoredAgentTask : AgentTask
    {
        [AutoIncrement]
        public int Id { get; set; }
        
        public string? Name { get; set; }

        public int StoredAgentDataId { get; set; }

        [Reference]
        public List<StoredChatMessage> ChatHistory { get; set; }
    }

    public class StoredAgentData : GptAgentData

    {
        [AutoIncrement]
        public int Id { get; set; }
    }

    public class StoredChatMessage : ChatMessage
    {
        [AutoIncrement]
        public int Id { get; set; }
        
        public bool ContentIsJson { get; set; }

        public int StoredAgentTaskId { get; set; }
    }
    
    public class StoredAgentCommand : AgentCommand
    {
        [AutoIncrement]
        public int Id { get; set; }
        public object Response { get; set; }
    
        public int StoredChatMessageId { get; set; }
    }
    
    public class CalendarEntry
    {
        [AutoIncrement]
        public int Id { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        
        [References(typeof(TeamUser))]
        public int AttendeeId { get; set; }
    
        [Reference]
        public TeamUser Attendee { get; set; }
    }
    
    public class TeamUser
    {
        [AutoIncrement]
        public int Id { get; set; }
        public string DisplayName { get; set; }
        public string Email { get; set; }
        public string UserPrincipalName { get; set; }
        public string ProfileUrl { get; set; }
    }
    
    public class ScheduleItem
    {
        [AutoIncrement]
        public int Id { get; set; }
        public bool? IsPrivate { get; set; }
        public string Status { get; set; }
        public string Subject { get; set; }
        public string Location { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public List<string> AttendeeEmails { get; set; }
    
        public int TeamUserId { get; set; }
    }

    public override void Up()
    {
        Db.CreateTable<StoredAgentData>();
        Db.CreateTable<StoredAgentTask>();
        Db.CreateTable<StoredChatMessage>();
        Db.CreateTable<StoredAgentCommand>();
        
        Db.CreateTable<TeamUser>();
        Db.CreateTable<ScheduleItem>();
        Db.CreateTable<CalendarEntry>();
        Db.SeedMeetingData();
    }
    
    public override void Down()
    {
        Db.DropTable<StoredAgentCommand>();
        Db.DropTable<StoredAgentTask>();
        Db.DropTable<StoredChatMessage>();
        Db.DropTable<StoredAgentData>();
        
        Db.DropTable<CalendarEntry>();
        Db.DropTable<ScheduleItem>();
        Db.DropTable<TeamUser>();
    }
}
