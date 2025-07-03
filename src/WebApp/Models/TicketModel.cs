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
    [Required]
    public string Title { get; set; }
    [StringLength(2000, ErrorMessage = "Die Beschreibung darf maximal 400 Zeichen lang sein.")]
    [Required]
    public string Description { get; set; }
    [Required]
    public DateTime CreatedAt { get; set; }
    [Required]
    public DateTime AssignedAt { get; set; }
    [Required]
    public TicketStatus Status { get; set; } = TicketStatus.Open;

    public int ProjectId { get; set; }
    [Required]
    public ProjectModel Project { get; set; }

    public string CreatorUserId { get; set; } = string.Empty;
    [Required]
    public AppUser CreatorUser { get; set; }

    public string? AssignedUserId { get; set; }
    public AppUser? AssignedUser { get; set; }

    public ICollection<TicketHistoryModel> History { get; set; } = new List<TicketHistoryModel>();

    public ICollection<TicketFile>? Files { get; set; }
    public ICollection<TicketComments> Comments { get; set; } = new List<TicketComments>();
}