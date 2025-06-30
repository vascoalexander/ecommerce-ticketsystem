namespace WebApp.ViewModels;

public class NewTicketInputModel
{
    public int? TicketId { get; set; }  // null for new tickets, set for editing
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";

    public string? AssignedUserId { get; set; } = "";

    public int ProjectId { get; set; }
    
    public string Status { get; set; } = "Open";  // default for new tickets
}
