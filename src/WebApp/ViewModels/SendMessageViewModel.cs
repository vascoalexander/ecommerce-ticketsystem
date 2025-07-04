using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebApp.ViewModels;

public class SendMessageViewModel
{
    [Required(ErrorMessage = "Bitte wählen Sie einen Empfänger aus.")]
    [Display(Name = "Empfänger")]
    public string ReceiverId { get; set; } = null!;

    public IEnumerable<SelectListItem>? AvailableReceivers { get; set; }

    [Required(ErrorMessage = "Bitte geben Sie einen Betreff ein.")]
    [StringLength(120, ErrorMessage = "Der Betreff darf maximal 120 Zeichen lang sein.")]
    [Display(Name = "Betreff")]
    public string Subject { get; set; } = null!;

    [Required(ErrorMessage = "Bitte geben Sie einen Nachrichtentext ein.")]
    [StringLength(2000, ErrorMessage = "Der Nachrichtentext darf maximal 2000 Zeichen lang sein.")]
    [Display(Name = "Nachricht")]
    [DataType(DataType.MultilineText)]
    public string Body { get; set; } = null!;
}