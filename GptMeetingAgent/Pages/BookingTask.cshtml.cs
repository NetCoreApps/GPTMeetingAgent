using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GptMeetingAgent.Pages;

public class BookingTaskModel : PageModel
{
    public int Id { get; set; }
    
    private readonly ILogger<BookingTaskModel> _logger;

    public BookingTaskModel(ILogger<BookingTaskModel> logger)
    {
        _logger = logger;
    }

    public void OnGet(int id)
    {
        Id = id;
    }
}