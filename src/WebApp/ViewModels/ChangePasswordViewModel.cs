using System.ComponentModel.DataAnnotations;

namespace WebApp.ViewModels;

public class ChangePasswordViewModel
{
    [Required(ErrorMessage = "Das aktuelle Passwort ist erforderlich!")]
    [DataType(DataType.Password)]
    [Display(Name = "Aktuelles Passwort")]
    public string OldPassword { get; set; } = null!;

    [Required(ErrorMessage = "Das neue Passwort ist erforderlich.")]
    [DataType(DataType.Password)]
    [Display(Name = "Neues Passwort")]
    public string NewPassword { get; set; } = null!;

    [DataType(DataType.Password)]
    [Display(Name = "Neues Passwort bestätigen")]
    [Compare("NewPassword", ErrorMessage = "Das neue Passwort und das Bestätigungspasswort stimmen nicht überein.")]
    public string ConfirmNewPassword { get; set; } = null!;
}