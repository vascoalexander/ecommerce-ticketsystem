using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace WebApp.Models
{
    public class AdminUserModel
    {
        public string? Id { get; set; }
        public string? UserName { get; set; }
        [Required(ErrorMessage = "Passwort ist erforderlich.")]
        [DataType(DataType.Password)]
        public string? Password { get; set; }
        public IList<SelectListItem> Roles { get; set; } = new List<SelectListItem>();
        [Required(ErrorMessage = "Rolle muss zugewiesen werden.")]
        public string? Role { get; set; }
    }
}
