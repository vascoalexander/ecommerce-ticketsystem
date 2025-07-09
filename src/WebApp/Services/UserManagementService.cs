using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebApp.Models;

namespace WebApp.Services;

public class UserManagementService : IUserManagementService
{
    private readonly UserManager<AppUser> _userManager;

    public UserManagementService(UserManager<AppUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<AppUser?> GetUserByIdAsync(string userId)
    {
        return await _userManager.FindByIdAsync(userId);
    }

    public async Task<IEnumerable<SelectListItem>> GetAvailableReceiversAsync(string currentUserId)
    {
        return await _userManager.Users
            .Where(u => u.Id != currentUserId)
            .OrderBy(u => u.UserName)
            .Select(u => new SelectListItem { Text = u.UserName, Value = u.Id.ToString() })
            .ToListAsync();
    }
}