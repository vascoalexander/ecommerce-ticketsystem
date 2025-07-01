using System.ComponentModel.DataAnnotations;

namespace WebApp.ViewModels;

public class NewTicketInputModel
{
    public int? TicketId { get; set; }  // null for new tickets, set for editing
    [StringLength(60, ErrorMessage = "Der Titel darf maximal 60 Zeichen lang sein.")]
    public string Title { get; set; } = "";
    [StringLength(2000, ErrorMessage = "Die Beschreibung darf maximal 2000 Zeichen lang sein.")]
    public string Description { get; set; } = "";

    public string? AssignedUserId { get; set; } = "";

    public int ProjectId { get; set; }

    public string Status { get; set; } = "Open";  // default for new tickets
}
