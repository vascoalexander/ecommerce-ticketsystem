using WebApp.Models;

namespace WebApp.ViewModels;

public class TicketDetailViewModel
{
    public int TicketId { get; set; }
    
    public TicketModel? Ticket { get; set; }
    public List<TicketHistoryModel>? History { get; set; }
    public string NewCommentContent { get; set; } = string.Empty;
    public List<TicketComments>? Comments { get; set; }
}