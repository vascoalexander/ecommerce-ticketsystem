using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebApp.Models;

public class ProjectModel
{
    public int Id { get; set; }
    [StringLength(40, ErrorMessage = "Der Titel darf maximal 40 Zeichen lang sein.")]
    public required string Title { get; set; }
    [StringLength(2000, ErrorMessage = "Die Beschreibung darf maximal 400 Zeichen lang sein.")]
    public required string Description { get; set; }

    [StringLength(40)]
    public string? Category { get; set; } = String.Empty;

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public virtual ICollection<TicketModel> Tickets { get; set; } = new List<TicketModel>();
 [DefaultValue(true)] 
 public bool ProjectActive { get; set; } = true;



}
