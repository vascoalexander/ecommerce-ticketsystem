using WebApp.Models;

namespace WebApp.ViewModels;

public class ProjectDetailViewModel
{
    public ProjectModel Project { get; set; } = null!;
    public List<TicketModel> Tickets { get; set; } = null!;
}