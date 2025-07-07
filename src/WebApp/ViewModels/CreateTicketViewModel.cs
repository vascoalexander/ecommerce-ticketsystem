using System.ComponentModel.DataAnnotations;
using WebApp.Models;

namespace WebApp.ViewModels;

public class CreateTicketViewModel
{
    [Required(ErrorMessage = "Titel ist erforderlich.")]
    [StringLength(100, ErrorMessage = "Titel darf maximal 100 Zeichen lang sein.")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Beschreibung ist erforderlich.")]
    [StringLength(1000, ErrorMessage = "Beschreibung darf maximal 1000 Zeichen lang sein.")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Projekt muss ausgewählt werden.")]
    public int ProjectId { get; set; }

    public string AssignedUserId { get; set; } = null!;

    // Für Dropdowns
    public List<ProjectModel> AvailableProjects { get; set; } = new();
    public List<AppUser> AvailableUsers { get; set; } = new();
}
