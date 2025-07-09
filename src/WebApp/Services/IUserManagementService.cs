using Microsoft.AspNetCore.Mvc.Rendering;
using WebApp.Models;

namespace WebApp.Services;

public interface IUserManagementService
{
    Task<AppUser?> GetUserByIdAsync(string userId);
    Task<IEnumerable<SelectListItem>> GetAvailableReceiversAsync(String currentUserId);
}