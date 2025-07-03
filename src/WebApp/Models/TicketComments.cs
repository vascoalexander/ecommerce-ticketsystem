using System.ComponentModel.DataAnnotations;

namespace WebApp.Models;

public class TicketComments
{
    public int Id { get; set; }
    [StringLength(4000, ErrorMessage = "Die Beschreibung darf maximal 4000 Zeichen lang sein.")]
    public string? Content { get; set; }
    public int TicketId { get; set; }
    public TicketModel? Ticket { get; set; }
    public string CreatorUserId { get; set; } = string.Empty;
    public AppUser? CreatorUser { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}