using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebApp.Models;

namespace WebApp.Services;

public class UserManagementService : IUserManagementService
{
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly UserManager<AppUser> _userManager;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserManagementService(UserManager<AppUser> userManager, IHttpContextAccessor httpContextAccessor, SignInManager<AppUser> signInManager, RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
        _httpContextAccessor = httpContextAccessor;
    }

    public IQueryable<AppUser> GetAllUsersQuery()
    {
        return _userManager.Users;
    }

    public async Task<AppUser?> GetUserByIdAsync(string userId)
    {
        return await _userManager.FindByIdAsync(userId);
    }

    public async Task<AppUser?> GetUserByNameAsync(string userName)
    {
        return await _userManager.FindByNameAsync(userName);
    }

    public async Task<AppUser?> GetCurrentUserAsync()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext?.User == null)
            return null;

        return await _userManager.GetUserAsync(httpContext.User);
    }

    public Task<string?> GetCurrentUserIdAsync()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext?.User == null)
            return Task.FromResult<string?>(null);

        return Task.FromResult(_userManager.GetUserId(httpContext.User));
    }

    public async Task<SignInResult> UserSignInAsync(string userEmail, string password, bool isPersistent, bool lockoutOnFailure)
    {
        var user = await _userManager.FindByEmailAsync(userEmail);

        if (user != null && !user.IsActive)
        {
            throw new InvalidOperationException("Ihr Benutzerkonto ist deaktiviert.");
        }

        var result = await _signInManager.PasswordSignInAsync(userEmail, password, isPersistent, lockoutOnFailure);
        return result;
    }

    public Task UserSignOutAsync()
    {
        throw new NotImplementedException();
    }

    public Task<bool> IsSignedInAsync(ClaimsPrincipal userPrincipal)
    {
        throw new NotImplementedException();
    }

    public Task<bool> IsUserInRoleAsync(string userId, string roleName)
    {
        throw new NotImplementedException();
    }

    public Task<IdentityResult> AddUserToRoleAsync(string userId, string roleName)
    {
        throw new NotImplementedException();
    }

    public Task<IdentityResult> RemoveUserFromRoleAsync(string userId, string roleName)
    {
        throw new NotImplementedException();
    }

    public Task<IList<string>> GetUserRolesAsync(string userId)
    {
        throw new NotImplementedException();
    }
}