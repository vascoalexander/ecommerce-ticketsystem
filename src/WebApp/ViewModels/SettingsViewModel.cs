namespace WebApp.ViewModels;

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering; // Für SelectListItem

public enum ThemeOption
{
    [Display(Name = "standard")] Standard,
    [Display(Name = "lushwear")] Lushwear
}

public class SettingsViewModel
{
    [Required(ErrorMessage = "Benutzername ist erforderlich.")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Benutzername muss zwischen 3 und 50 Zeichen lang sein.")]
    [Display(Name = "Benutzername")]
    public string Username { get; set; } = null!;

    [Required(ErrorMessage = "E-Mail ist erforderlich.")]
    [EmailAddress(ErrorMessage = "Ungültiges E-Mail-Format.")]
    [Display(Name = "E-Mail-Adresse")]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "Theme ist erforderlich.")]
    [Display(Name = "Theme auswählen")]
    public string SelectedTheme { get; set; } = null!;

    public List<SelectListItem>? AvailableThemes { get; set; }

    public string StatusMessage { get; set; } = "";
    public bool IsSuccess { get; set; } = false;
    
    
}