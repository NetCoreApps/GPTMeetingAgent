using ServiceStack.DataAnnotations;
using ServiceStack.OrmLite;

namespace GptMeetingAgent.Migrations;

public class Migration1001 : MigrationBase
{
    public class MeetingRoom
    {
        [AutoIncrement]
        public int Id { get; set; }
        
        public string Name { get; set; }
        public string Location { get; set; }
        public List<string> Resources { get; set; }
    }
    
    public class CalendarEntry
    {
        public int? MeetingRoomId { get; set; }
    }
    
    public override void Up()
    {
        Db.CreateTable<MeetingRoom>();
        Db.Migrate<CalendarEntry>();
        Db.SeedMeetingRoomData();
    }

    public override void Down()
    {
        Db.Revert<CalendarEntry>();
        Db.DropTable<MeetingRoom>();
    }
}

