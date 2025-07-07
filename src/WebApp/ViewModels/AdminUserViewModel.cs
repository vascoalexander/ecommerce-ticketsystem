using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace WebApp.ViewModels
{
    public class AdminUserViewModel
    {
        public string? Id { get; set; }
        [Required(ErrorMessage = "Name ist erforderlich")]
        public string? UserName { get; set; }
     
        [DataType(DataType.Password)]
        public string? Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Neues Passwort bestätigen")]

        [Compare("Password", ErrorMessage = "Das neue Passwort und das Bestätigungspasswort stimmen nicht überein.")]
        public string ConfirmNewPassword { get; set; } = null!;
        public string? Email { get; set; }

        public IList<string> AssignedRoles { get; set; } = new List<string>();

        public bool IsActive { get; set; } = true;

        public List<SelectListItem> AvailableRoles { get; set; } = new();
        [Required(ErrorMessage = "Rolle muss zugewiesen werden.")]
        public string? SelectedRole { get; set; }


    }
}
