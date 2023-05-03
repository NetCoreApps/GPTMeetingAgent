using ServiceStack;
using ServiceStack.DataAnnotations;

namespace GptMeetingAgent.ServiceModel;

public class TeamUser
{
    [AutoIncrement]
    public int Id { get; set; }
    public string DisplayName { get; set; }
    public string Email { get; set; }
    public string UserPrincipalName { get; set; }
    public string ProfileUrl { get; set; }
}

[Tag("Teams"), Description("Search for users by first or last name and get back their IDs and emails")]
public class SearchUsers : IReturn<SearchUsersResponse>
{
    public string Name { get; set; }
}

public class SearchUsersResponse
{
    public List<TeamUser> Value { get; set; }
}

public class QueryTeamUser : QueryDb<TeamUser>
{
}


[Tag("Teams"), Description("Get a user's schedule")]
//[ConfirmationRequired("Are you sure you want to get user schedule?")]
public class GetUserSchedule : IReturn<GetUserScheduleResponse>
{
    public int UserId { get; set; }
    public List<string> Schedules { get; set; }
    [Input(Type = "datetime-local")]
    public DateTime StartTime { get; set; }
    [Input(Type = "datetime-local")]
    public DateTime EndTime { get; set; }
    public int AvailabilityViewInterval { get; set; }
}

public class GetUserScheduleResponse
{
    public string ODataContext { get; set; }
    public List<ScheduleInformation> Value { get; set; }
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

public class TimeZoneInfo
{
    public string Name { get; set; }
    public int? Bias { get; set; }
    public string ODataType { get; set; }
    public DateTimeOffset StandardOffset { get; set; }
    public DateTimeOffset DaylightOffset { get; set; }
    public int? DaylightBias { get; set; }
}

public class WorkingHours
{
    public List<string> DaysOfWeek { get; set; }
    public string StartTime { get; set; }
    public string EndTime { get; set; }
    public TimeZoneInfo TimeZone { get; set; }
}

public class ScheduleInformation
{
    public string ScheduleId { get; set; }
    public string AvailabilityView { get; set; }
    public List<ScheduleItem> ScheduleItems { get; set; }
    public WorkingHours WorkingHours { get; set; }
}


public class QueryCalendarEvents : QueryDb<CalendarEntry>
{
}

public class CalendarEntry
{
    [AutoIncrement]
    public int Id { get; set; }
    public string Subject { get; set; }
    public string Body { get; set; }
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    
    public int? MeetingRoomId { get; set; }
    [References(typeof(TeamUser))]
    public int AttendeeId { get; set; }
    
    [Reference]
    public TeamUser Attendee { get; set; }
}


public class MeetingRoom
{
    [AutoIncrement]
    public int Id { get; set; }
        
    public string Name { get; set; }
    public string Location { get; set; }
    public List<string> Resources { get; set; }
}

[Tag("Teams"), Description("Get a list of meeting rooms and their resources.")]
 public class ListMeetingRooms : QueryDb<MeetingRoom>
 {
     
 }
