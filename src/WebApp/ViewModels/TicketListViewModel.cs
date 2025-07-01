using Microsoft.AspNetCore.Identity;
using WebApp.Models;

namespace WebApp.ViewModels;

public class TicketListViewModel
{
    public List<TicketModel> Tickets { get; set; } = new();
    public NewTicketInputModel NewTicket { get; set; } = new();
    public List<ProjectModel> AvailableProjects { get; set; } = new(); // muss NICHT null sein!
    public List<AppUser> AvailableUsers { get; set; } = new();
    public string? SelectedStatus { get; set; }
    public string? Search { get; set; }
    public string? SelectedCreatorId { get; set; }
    public string? SelectedAssigneeId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public List<TicketStatus> StatusOptions { get; set; } = new();


}

