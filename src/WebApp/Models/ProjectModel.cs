using System.ComponentModel.DataAnnotations;

namespace WebApp.Models;

public class ProjectModel
{
    public int Id { get; set; }
    [StringLength(20, ErrorMessage = "Der Titel darf maximal 20 Zeichen lang sein.")]
    public required string Title { get; set; }
    [StringLength(400, ErrorMessage = "Die Beschreibung darf maximal 400 Zeichen lang sein.")]
    public required string Description { get; set; }

    [StringLength(20)]
    public string? Category { get; set; } = String.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public virtual ICollection<TicketModel>? Tickets { get; set; }
}