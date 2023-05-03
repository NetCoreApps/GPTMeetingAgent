using GptMeetingAgent.ServiceModel;
using ServiceStack;
using ServiceStack.DataAnnotations;
using ServiceStack.OrmLite;

namespace GptMeetingAgent.ServiceInterface;

public class MeetingEventService : Service
{
    public async Task<object> Post(CreateCalendarEvent request)
    {
        var teamUser = await Db.SingleAsync<TeamUser>(x => x.Email == request.AttendeeEmail);
        var calendarEvent = request.ConvertTo<CalendarEntry>();
        calendarEvent.AttendeeId = teamUser.Id;
        var id = await Db.InsertAsync(calendarEvent, selectIdentity:true);
        return new CreateCalendarEventResponse
        {
            Id = id.ToString(),
            Body = request.Body,
            Subject = request.Subject,
            Attendee = new() { request.AttendeeEmail },
            Start = request.Start,
            End = request.End
        };
    }
}



