using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
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
    Task<SignInResult> UserSignInAsync(string userEmail, string password, bool isPersistent, bool lockoutOnFailure);
    Task UserSignOutAsync();
    Task<bool> IsSignedInAsync(ClaimsPrincipal userPrincipal);
    Task<bool> IsUserInRoleAsync(string userId, string roleName);
    Task<IdentityResult> AddUserToRoleAsync(string userId, string roleName);
    Task<IdentityResult> RemoveUserFromRoleAsync(string userId, string roleName);
    Task<IList<string>> GetUserRolesAsync(string userId);
}