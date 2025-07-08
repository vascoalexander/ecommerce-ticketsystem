using WebApp.Models;

namespace WebApp.ViewModels;

public class TicketDetailViewModel
{
    public TicketModel? Ticket { get; set; }
    public List<TicketHistoryModel>? History { get; set; }
    public string NewCommentContent { get; set; } = string.Empty;
    public List<TicketComments>? Comments { get; set; }
}