using Microsoft.AspNetCore.Identity;
using WebApp.Models;

namespace WebApp.ViewModels;

public class TicketDetailViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string ProjectTitle { get; set; } = "";
    public string Creator { get; set; } = "";
    public DateTime CreatedAt { get; set; }
    public string Assignee { get; set; } = "";
    public DateTime? AssignedAt { get; set; }
    public string Description { get; set; } = "";
}