using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace WebApp.ViewModels
{
    public class AdminUserViewModel
    {
        public string? Id { get; set; }
        [Required(ErrorMessage = "Name ist erforderlich")]
        public string? UserName { get; set; }
        [Required(ErrorMessage = "Passwort ist erforderlich.")]
        [DataType(DataType.Password)]
        public string? Password { get; set; }
        public IList<string> AssignedRoles { get; set; } = new List<string>();


        public List<SelectListItem> AvailableRoles { get; set; } = new();
        [Required(ErrorMessage = "Rolle muss zugewiesen werden.")]
        public string? SelectedRole { get; set; }
    }
}
