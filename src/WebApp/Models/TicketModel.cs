using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace WebApp.Models;

public enum TicketStatus
{
    [Display(Name = "Open")]
    Open,
    [Display(Name = "InProgress")]
    InProgress,
    [Display(Name = "Closed")]
    Closed
}
public class TicketModel
{
    public int Id { get; set; }
    [StringLength(60, ErrorMessage = "Der Titel darf maximal 60 Zeichen lang sein.")]
    public required string Title { get; set; }
    [StringLength(2000, ErrorMessage = "Die Beschreibung darf maximal 400 Zeichen lang sein.")]
    public required string Description { get; set; }
    public required DateTime CreatedAt { get; set; }
    public required DateTime AssignedAt { get; set; }
    public required TicketStatus Status { get; set; } = TicketStatus.Open;

    public int ProjectId { get; set; }
    public required ProjectModel Project { get; set; }

    public string CreatorUserId { get; set; } = string.Empty;
    public required AppUser CreatorUser { get; set; }

    public string? AssignedUserId { get; set; }
    public AppUser? AssignedUser { get; set; }
}