using System.ComponentModel.DataAnnotations;

namespace WebApp.Models;

public class LoginViewModel
{
    [Required]
    public required string Email { get; set; }
    [Required]
    [DataType(DataType.Password)]
    public required string Password { get; set; }
    public string? ErrorMessage { get; set; }
    public string? ReturnUrl { get; set; } = "/";
}