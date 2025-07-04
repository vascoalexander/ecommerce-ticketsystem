using Microsoft.AspNetCore.Identity;
using WebApp.Models;

namespace WebApp.Helper;

public static class Utility
{
    public static string Truncate(string s, int maxLength)
    {
        if (!string.IsNullOrEmpty(s) && s.Length > maxLength)
        {
            return s.Substring(0, maxLength);
        }
        return s;
    }
    
    public static async Task<List<AppUser>> GetUsersExcludingSystemAsync(UserManager<AppUser> userManager)
    {
        var systemUser = await userManager.FindByNameAsync("system");

        var allUsers = userManager.Users.ToList();

        if (systemUser != null)
            return allUsers.Where(u => u.Id != systemUser.Id).ToList();

        return allUsers;
    }
}