using Microsoft.AspNetCore.Identity;
using WebApp.Models;

namespace WebApp.Repositories;

public class AccountRepository
{
    private readonly UserManager<AppUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public AccountRepository(UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager)

    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public List<AppUser> GetAllUsers() => _userManager.Users.ToList();

    public async Task<AppUser?> GetUserById(string Id) => await _userManager.FindByIdAsync(Id);
    public async Task<AppUser?> GetUserByName(string username) => await _userManager.FindByNameAsync(username);

    public async Task<IdentityResult> CreateUser(string username, string password, string role)
    {
        var user = new AppUser(username);
        var result = await _userManager.CreateAsync(user, password);
        if (result.Succeeded)
        {
            var roleResult = await _userManager.AddToRoleAsync(user, role);
            if (!roleResult.Succeeded)
            {
                return IdentityResult.Failed(roleResult.Errors.ToArray());
            }
        }

        return result;
    }

    public async Task<IdentityResult> DeleteUser(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user != null)
        {
            return await _userManager.DeleteAsync(user);
        }

        return IdentityResult.Failed(new IdentityError { Description = "User nicht gefunden." });
    }

    public async Task<IList<string>> GetUserRoles(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        return user != null ? await _userManager.GetRolesAsync(user) : new List<string>();
    }

    public async Task<IdentityResult> ChangeUserRole(string userId, string newrole)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user != null)
        {
            return IdentityResult.Failed(new IdentityError { Description = "User not found." });
        }

        var currentRoles = await _userManager.GetRolesAsync(user);
        var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
        if (!removeResult.Succeeded)
        {
            return removeResult;
        }

        return await _userManager.AddToRoleAsync(user, newrole);
    }

    public List<IdentityRole> GetRoles() => _roleManager.Roles.ToList();
}