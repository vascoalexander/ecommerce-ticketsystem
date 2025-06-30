using Microsoft.AspNetCore.Identity;
using WebApp.Models;

namespace WebApp.ViewModels;

public class TicketListViewModel
{
    public List<TicketModel> Tickets { get; set; } = new();
    public NewTicketInputModel NewTicket { get; set; } = new();
    public List<ProjectModel> AvailableProjects { get; set; } = new();  // muss NICHT null sein!
    public List<AppUser> AvailableUsers { get; set; } = new();
}
