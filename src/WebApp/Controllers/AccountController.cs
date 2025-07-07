using System.ComponentModel.DataAnnotations;
using System.Reflection;
using WebApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using WebApp.Helper;
using WebApp.Repositories;
using WebApp.ViewModels;

namespace WebApp.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly MessageRepository _messageRepository;

    public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, MessageRepository messageRepository)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _messageRepository = messageRepository;
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
                if (!user.IsActive)
                {
                    viewModel.ErrorMessage = "Ihr Benutzerkonto ist deaktiviert.";
                    return View(viewModel);
                }
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
                else
                {
                    viewModel.ErrorMessage = "Benutzername oder Passwort ist falsch.";
                    return View(viewModel);
                }
            }
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

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Settings()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) { return View("Login"); }

        var model = new SettingsViewModel()
        {
            Username = user.UserName!,
            Email = user.Email!,
            SelectedTheme = user.UserTheme,
            AvailableThemes = GetAvailableThemes()
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Settings(SettingsViewModel model)
    {
        model.AvailableThemes = GetAvailableThemes();
        if (!ModelState.IsValid)
        {
            model.StatusMessage = "Bitte korrigieren Sie die Fehler im Formular.";
            model.IsSuccess = false;
            return View(model);
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound();
        }

        var setEmailResult = await _userManager.SetEmailAsync(user, model.Email);
        if (!setEmailResult.Succeeded)
        {
            AddErrors(setEmailResult);
            model.StatusMessage = "Fehler beim Aktualisieren der E-Mail-Adresse.";
            model.IsSuccess = false;
            return View(model);
        }

        var setUserNameResult = await _userManager.SetUserNameAsync(user, model.Username);
        if (!setUserNameResult.Succeeded)
        {
            AddErrors(setUserNameResult);
            model.StatusMessage = "Fehler beim Aktualisieren des Benutzernamens.";
            model.IsSuccess = false;
            return View(model);
        }

        if (user.UserTheme != model.SelectedTheme)
        {
            user.UserTheme = model.SelectedTheme;
            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                AddErrors(updateResult);
                model.StatusMessage = "Fehler beim Aktualisieren des Themes.";
                model.IsSuccess = false;
                return View(model);
            }
        }
        model.StatusMessage = "Ihre Einstellungen wurden erfolgreich gespeichert!";
        model.IsSuccess = true;

        return View(model);
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Messages()
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) { return View("Login"); }

        var model = new MessagesViewModel()
        {
            ReceivedMessages = await _messageRepository.GetMessagesReceived(currentUser.Id),
            SentMessages = await _messageRepository.GetMessagesSent(currentUser.Id),
            SystemMessages = await _messageRepository.GetSystemMessageReceived(currentUser.Id)
        };

        return View(model);
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> MessageDetails(int messageId)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) { return View("Login"); }

        var message = await _messageRepository.GetMessageById(messageId);
        if (message == null) { return View("Messages"); }

        if (message.SenderId != currentUser.Id && message.ReceiverId != currentUser.Id)
        {
            return Forbid();
        }

        if (message.ReceiverId == currentUser.Id && !message.IsRead)
        {
            message.IsRead = true;
            _messageRepository.UpdateMessage(message);
            await _messageRepository.SaveChangesAsync();
        }
        return View("MessageDetails", message);
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> SendMessage(int? replyToMessageId)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) { return View("Login"); }

        var model = new SendMessageViewModel();
        model.AvailableReceivers = await GetAvailableReceivers(currentUser.Id);

        if (replyToMessageId.HasValue)
        {
            var originalMessage = await _messageRepository.GetMessageById(replyToMessageId.Value);
            if (originalMessage != null && originalMessage.ReceiverId == currentUser.Id)
            {
                model.ReceiverId = originalMessage.SenderId;
                if (!string.IsNullOrEmpty(originalMessage.Subject) && !originalMessage.Subject.StartsWith("Re: "))
                {
                    model.Subject = $"Re: {originalMessage.Subject}";
                }
                else if (!string.IsNullOrEmpty(originalMessage.Subject))
                {
                    model.Subject = originalMessage.Subject;
                }
            }
        }
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SendMessage(SendMessageViewModel model)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) { return View("Login"); }

        if (!ModelState.IsValid)
        {
            model.AvailableReceivers = await GetAvailableReceivers(currentUser.Id);
            return View(model);
        }

        var receiverUser = await _userManager.FindByIdAsync(model.ReceiverId);
        if (receiverUser == null)
        {
            ModelState.AddModelError("ReceiverId", "Der ausgewählte Empfänger existiert nicht.");
            model.AvailableReceivers = await GetAvailableReceivers(currentUser.Id);
            return View(model);
        }

        var message = new Message
        {
            SenderId = currentUser.Id,
            Sender = currentUser,
            ReceiverId = model.ReceiverId,
            Receiver = receiverUser,
            Body = model.Body,
            Subject = model.Subject,
            SentAt = DateTime.UtcNow,
            IsRead = false
        };
        await _messageRepository.AddMessage(message);
        await _messageRepository.SaveChangesAsync();
        return RedirectToAction(nameof(Messages));
    }

    private List<SelectListItem> GetAvailableThemes()
    {
        var themeOptions = Enum.GetValues(typeof(ThemeOption))
            .Cast<ThemeOption>()
            .Select(e => new SelectListItem
            {
                Value = e.ToString(),
                Text = e.GetType()
                    .GetMember(e.ToString())
                    .First()
                    .GetCustomAttribute<DisplayAttribute>()?
                    .GetName() ?? e.ToString()
            }).ToList();
        return themeOptions;
    }

    private async Task<IEnumerable<SelectListItem>> GetAvailableReceivers(String currentUserId)
    {
        // Get all users excluding system user
        var users = await Utility.GetUsersExcludingSystemAsync(_userManager);

        // Filter out current user and order by username, project to SelectListItem
        var filteredUsers = users
            .Where(u => u.Id != currentUserId)
            .OrderBy(u => u.UserName)
            .Select(u => new SelectListItem
            {
                Value = u.Id,
                Text = u.UserName
            });

        return filteredUsers;
    }
    private void AddErrors(IdentityResult result)
    {
        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }
    }
}