using Microsoft.AspNetCore.Identity;
using WebApp.Models;

namespace WebApp.Data;

public static class EnsureIdentity
{
    private const string AdminRole = "Admin";
    private const string TesterRole = "Tester";
    private const string DeveloperRole = "Developer";
    private const string SystemRole = "System";

    private const string AdminName = "admin";
    private const string TesterName = "tester";
    private const string DeveloperName = "developer";
    private const string SystemName = "system";

    private const string AdminPassword = "Admin123$";
    private const string TesterPassword = "Tester123$";
    private const string DeveloperPassword = "Developer123$";
    private const string SystemPassword = "System123$";

    private const string AdminEmail = "admin@lushwear.com";
    private const string TesterEmail = "tester@lushwear.com";
    private const string DeveloperEmail = "developer@lushwear.com";
    private const string SystemEmail = "system@lushwear.com";

    public static async Task SeedDefaultAccounts(IApplicationBuilder app)
    {
        RoleManager<IdentityRole> roleManager = app.ApplicationServices
            .CreateScope()
            .ServiceProvider
            .GetRequiredService<RoleManager<IdentityRole>>();
        UserManager<AppUser> userManager = app.ApplicationServices
            .CreateScope()
            .ServiceProvider
            .GetRequiredService<UserManager<AppUser>>();

        if (!await roleManager.RoleExistsAsync(AdminRole))
        {
            IdentityRole role = new IdentityRole(AdminRole);
            await roleManager.CreateAsync(role);
        }
        if (!await roleManager.RoleExistsAsync(TesterRole))
        {
            IdentityRole role = new IdentityRole(TesterRole);
            await roleManager.CreateAsync(role);
        }
        if (!await roleManager.RoleExistsAsync(DeveloperRole))
        {
            IdentityRole role = new IdentityRole(DeveloperRole);
            await roleManager.CreateAsync(role);
        }

        if (!await roleManager.RoleExistsAsync(SystemRole))
        {
            IdentityRole role = new IdentityRole(SystemRole);
            await roleManager.CreateAsync(role);
        }

        AppUser? admin = await userManager.FindByNameAsync(AdminName);
        if (admin == null)
        {
            admin = new AppUser(AdminName)
            {
                Email = AdminEmail,
                EmailConfirmed = true,
            };
            await userManager.CreateAsync(admin, AdminPassword);
            await userManager.AddToRoleAsync(admin, AdminRole);
            await userManager.AddToRoleAsync(admin, TesterRole);
            await userManager.AddToRoleAsync(admin, DeveloperRole);
        }
        AppUser? tester = await userManager.FindByNameAsync(TesterName);
        if (tester == null)
        {
            tester = new AppUser(TesterName)
            {
                Email = TesterEmail,
                EmailConfirmed = true,
            };
            await userManager.CreateAsync(tester, TesterPassword);
            await userManager.AddToRoleAsync(tester, TesterRole);
        }
        AppUser? developer = await userManager.FindByNameAsync(DeveloperName);
        if (developer == null)
        {
            developer = new AppUser(DeveloperName)
            {
                Email = DeveloperEmail,
                EmailConfirmed = true,
            };
            await userManager.CreateAsync(developer, DeveloperPassword);
            await userManager.AddToRoleAsync(developer, DeveloperRole);
        }
        AppUser? system = await userManager.FindByNameAsync(SystemName);
        if (system == null)
        {
            system = new AppUser(SystemName)
            {
                Email = SystemEmail,
                EmailConfirmed = true,
            };
            await userManager.CreateAsync(system, SystemPassword);
            await userManager.AddToRoleAsync(system, SystemRole);
        }

        if (string.IsNullOrEmpty(admin.Email) || admin.Email != AdminEmail)
        {
            admin.Email = AdminEmail;
            admin.EmailConfirmed = true;
            await userManager.UpdateAsync(admin);
        }

        if (string.IsNullOrEmpty(tester.Email) || tester.Email != TesterEmail)
        {
            tester.Email = TesterEmail;
            tester.EmailConfirmed = true;
            await userManager.UpdateAsync(tester);
        }

        if (string.IsNullOrEmpty(developer.Email) || developer.Email != DeveloperEmail)
        {
            developer.Email = DeveloperEmail;
            developer.EmailConfirmed = true;
            await userManager.UpdateAsync(developer);
        }

        if (string.IsNullOrEmpty(system.Email) || system.Email != SystemEmail)
        {
            system.Email = SystemEmail;
            system.EmailConfirmed = true;
            await userManager.UpdateAsync(system);
        }
    }
}