using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Rendering;
using WebApp.Models;

namespace WebApp.Services;

public interface IUserManagementService
{
    IQueryable<AppUser> GetAllUsersQuery();
    Task<AppUser?> GetUserByIdAsync(string userId);
    Task<AppUser?> GetUserByNameAsync(string userName);
    Task<AppUser?> GetCurrentUserAsync();
    Task<string?> GetCurrentUserIdAsync();
}