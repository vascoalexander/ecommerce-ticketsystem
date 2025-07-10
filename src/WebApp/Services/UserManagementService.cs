using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebApp.Models;

namespace WebApp.Services;

public class UserManagementService : IUserManagementService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserManagementService(UserManager<AppUser> userManager, IHttpContextAccessor httpContextAccessor)
    {
        _userManager = userManager;
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
}