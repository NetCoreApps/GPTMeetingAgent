using System.Data;
using Bogus;
using GptMeetingAgent.ServiceModel;
using ServiceStack.Data;
using ServiceStack.DataAnnotations;
using ServiceStack.OrmLite;

namespace GptMeetingAgent;

public static class ConfigureDbMeetingData
{
    public static void SeedMeetingData(this IDbConnection? db)
    {
        var seed = 1807832753;
        Randomizer.Seed = new Random(seed);
        if (db == null)
            return;

        var teamUserFaker = new Faker<TeamUser>()
            .RuleFor(o => o.Id, f => f.UniqueIndex)
            .RuleFor(o => o.DisplayName, f => f.Person.FullName)
            .RuleFor(o => o.Email, f => f.Person.Email)
            .RuleFor(o => o.UserPrincipalName, f => f.Person.UserName);

        var teamUsers = teamUserFaker.Generate(10).Select(x =>
        {
            x.ProfileUrl = $"/img/users/{x.Id + 1}.jpg";
            return x;
        }).ToList();
        db.InsertAll(teamUsers);
        
        var scheduleItemFaker = new Faker<ScheduleItem>()
            .RuleFor(o => o.Id, f => f.UniqueIndex)
            .RuleFor(o => o.Subject, f => f.Commerce.ProductName())
            .RuleFor(o => o.Start, f => f.Date.Recent())
            .RuleFor(o => o.End, f => f.Date.Soon())
            .RuleFor(o => o.Location, f => f.Address.FullAddress())
            .RuleFor(o => o.TeamUserId, f => teamUsers[f.Random.Int(0, 9)].Id)
            .RuleFor(o => o.Status, f => f.PickRandom("Busy", "Free", "Tentative"));
            
        var scheduleItems = scheduleItemFaker.Generate(15);
        db.InsertAll(scheduleItems);
    }

    public static void SeedMeetingRoomData(this IDbConnection? db)
    {
        var seed = 1807832753;
        Randomizer.Seed = new Random(seed);
        if (db == null)
            return;
        
        var meetingRoomFaker = new Faker<MeetingRoom>()
            .RuleFor(o => o.Id, f => f.UniqueIndex)
            .RuleFor(o => o.Name, f => f.Commerce.ProductName())
            .RuleFor(o => o.Location, f => f.Locale)
            .RuleFor(o => o.Resources,
                f => f.Random.Int(0, 3).Times(f.PickRandom<MeetingRoomResource>).Select(Enum.GetName).ToList());
        
        var meetingRooms = meetingRoomFaker.Generate(5);
        db.InsertAll(meetingRooms);
    }
}

public enum MeetingRoomResource
{
    SmartBoard,
    Projector,
    Whiteboard,
    VideoConference,
    SpeakerPhone
}