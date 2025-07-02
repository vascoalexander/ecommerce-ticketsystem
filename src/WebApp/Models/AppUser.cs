using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace WebApp.Models;

public class AppUser : IdentityUser
{
    [DefaultValue("default")]
    [StringLength(20)]
    public string UserTheme { get; set; } = "standard";
    public ICollection<TicketModel>? Tickets { get; set; }
    [DefaultValue(true)]
    public bool IsActive { get; set; } = true;

    public AppUser() : base() { }
    public AppUser(string userName) : base(userName) { }
}