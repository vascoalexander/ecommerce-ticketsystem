using Microsoft.AspNetCore.Authentication;
using WebApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using WebApp.Repositories;
using WebApp.ViewModels;

namespace WebApp.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;

    public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        var model = new LoginViewModel()
        {
            Email = string.Empty,
            Password = string.Empty,
            ReturnUrl = returnUrl
        };
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel viewModel)
    {
        if (ModelState.IsValid)
        {
            var user = await _userManager.FindByEmailAsync(viewModel.Email);
            if (user != null)
            {
                await _signInManager.SignOutAsync();
                var result = await _signInManager
                    .PasswordSignInAsync(
                        user,
                        viewModel.Password,
                        false,
                        false
                    );
                if (result.Succeeded)
                {
                    return RedirectToAction("Dashboard", "Home");
                }
            }
            ModelState.AddModelError("", "Username oder Passwort ungültig.");
        }
        return View(viewModel);
    }
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("DefaultPage", "Home");
    }
    public IActionResult AccessDenied(string returnUrl)
    {
        return View("AccessDenied", returnUrl);
    }

    [Authorize]
    [HttpGet]
    public IActionResult ChangePassword()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null) { return View("Login"); }
        var changePasswordResult = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
        if (!changePasswordResult.Succeeded)
        {
            foreach (var error in changePasswordResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return View(model);
        }

        await _signInManager.RefreshSignInAsync(user);
        return View(nameof(ChangePasswordConfirmation));
    }

    [HttpGet]
    public IActionResult ChangePasswordConfirmation()
    {
        return View();
    }
}