namespace WebApp.ViewModels;

public class NewTicketInputModel
{
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    
    public string AssignedUserId { get; set; } = "";

    public int ProjectId { get; set; }
}
