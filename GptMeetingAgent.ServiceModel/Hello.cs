using ServiceStack;
using ServiceStack.DataAnnotations;

namespace GptMeetingAgent.ServiceModel;

[Route("/hello")]
[Route("/hello/{Name}")]
[Tag("Teams"),Description("A Hello World Service")]
public class Hello : IReturn<HelloResponse>
{
    public string? Name { get; set; }
}

public class HelloResponse
{
    public string Result { get; set; } = default!;
}