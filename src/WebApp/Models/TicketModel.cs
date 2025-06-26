using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace WebApp.Models;

public class TicketModel
{
    public int Id { get; set; }
    [StringLength(20, ErrorMessage = "Der Titel darf maximal 20 Zeichen lang sein.")]
    public required string Title { get; set; }
    [StringLength(400, ErrorMessage = "Die Beschreibung darf maximal 400 Zeichen lang sein.")]
    public required string Description { get; set; }
    public required DateTime CreatedAt { get; set; }
    public required DateTime AssignedAt { get; set; }

    public int ProjectId { get; set; }
    public required ProjectModel Project { get; set; }

    public string CreatorUserId { get; set; } = string.Empty;
    public required IdentityUser CreatorUser { get; set; }

    public string? AssignedUserId { get; set; }
    public IdentityUser? AssignedUser { get; set; }
}