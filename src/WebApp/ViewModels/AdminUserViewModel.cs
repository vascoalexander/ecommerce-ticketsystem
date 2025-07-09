using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using WebApp.Models;

namespace WebApp.ViewModels
{
    

public class AdminUserViewModel
{
    public string? Id { get; set; }

    [Required(ErrorMessage = "Name ist erforderlich")]
    public string? UserName { get; set; }

    [Required(ErrorMessage = "Email ist erforderlich")]
    public string? Email { get; set; }
    [Required]
    [DataType(DataType.Password)]
    public string? Password { get; set;} 

    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "Die Passwörter stimmen nicht überein.")]
    public string? ConfirmPassword { get; set; }
    

    public IList<string> AssignedRoles { get; set; } = new List<string>();
    public bool IsActive { get; set; } = true;
    public List<SelectListItem> AvailableRoles { get; set; } = new();

    [Required(ErrorMessage = "Rolle muss zugewiesen werden.")]
    public string? SelectedRole { get; set; }
}
}
