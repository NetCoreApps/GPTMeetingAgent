using GptMeetingAgent.ServiceModel;
using ServiceStack;
using ServiceStack.OrmLite;

namespace GptMeetingAgent.ServiceInterface;

public class MeetingService : Service
{
    public async Task<object> Post(SearchUsers request)
    {
        return new SearchUsersResponse
        {
            Value = await Db.SelectAsync<TeamUser>(x => x.DisplayName.Contains(request.Name) || x.Email.Contains(request.Name))
        };
    }
    
    public async Task<object> Post(GetUserSchedule request)
    {
        var scheduleForUser = 
            await Db.SelectAsync<ScheduleItem>(
                x => x.TeamUserId == request.UserId &&
                     x.Start > request.StartTime &&
                     x.End < request.EndTime);

        if (scheduleForUser.IsEmpty())
        {
            scheduleForUser.Add(new ScheduleItem
            {
                Start = request.StartTime,
                End = request.EndTime,
                Status = "Free",
                TeamUserId = request.UserId
            });
        }
        return new GetUserScheduleResponse
        {
            Value = new List<ScheduleInformation>
            {
                new()
                {
                    ScheduleItems = scheduleForUser
                }
            }
        };
    }
}

