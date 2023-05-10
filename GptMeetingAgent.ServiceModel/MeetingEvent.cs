using ServiceStack;
using ServiceStack.DataAnnotations;

namespace GptMeetingAgent.ServiceModel;

[Tag(Tags.Calendar), Description("Create a meeting")]
[ConfirmationRequired("Are you sure you want to create a meeting?")]
public class CreateCalendarEvent : IReturn<CreateCalendarEventResponse>
{
    public string Subject { get; set; }
    public string Body { get; set; }
    [Input(Type = "datetime-local")]
    public DateTime Start { get; set; }
    [Input(Type = "datetime-local")]
    public DateTime End { get; set; }
    public string AttendeeEmail { get; set; }
    public int? MeetingRoomId { get; set; }
}

// Response objects

public class CreateCalendarEventResponse
{
    public string Id { get; set; }
    public string Subject { get; set; }
    public string BodyPreview { get; set; }
    public string Body { get; set; }
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public string Location { get; set; }
    // public int? MeetingRoomId { get; set; }
    
    public List<string> Attendee { get; set; }
}

[AttributeUsage(AttributeTargets.Class)]
public class ConfirmationRequiredAttribute : MetadataAttributeBase
{
    public string Message { get; set; }
    
    public ConfirmationRequiredAttribute(string message)
    {
        Message = message;
    }
}