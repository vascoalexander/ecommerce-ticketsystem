using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using WebApp.Models;

namespace WebApp.Helper;

public class ThemeFilter : IAsyncActionFilter
{
    private readonly UserManager<AppUser> _userManager;

    public ThemeFilter(UserManager<AppUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var controller = context.Controller as Controller;

        if (controller != null && controller.User.Identity.IsAuthenticated)
        {
            var user = await _userManager.GetUserAsync(controller.User);
            controller.ViewData["UserTheme"] = user?.UserTheme ?? "standard";
        }
        else
        {
            controller?.ViewData.Add("UserTheme", "standard");
        }

        await next();
    }
}