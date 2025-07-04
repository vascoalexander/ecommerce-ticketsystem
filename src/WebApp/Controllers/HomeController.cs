using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebApp.Models;
using WebApp.Repositories;
using WebApp.ViewModels;

namespace WebApp.Controllers;

public class HomeController : Controller
{
    private readonly TicketRepository _ticketRepository;
    private readonly UserManager<AppUser> _userManager;

    public HomeController(TicketRepository ticketRepository, UserManager<AppUser> userManager)
    {
        _ticketRepository = ticketRepository;
        _userManager = userManager;
    }

    public IActionResult DefaultPage()
    {
        return View();
    }

    [Authorize]
    public async Task<IActionResult> Dashboard()
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return RedirectToAction("DefaultPage");

        var userTickets = await _ticketRepository.GetTicketsForUserAsync(currentUser.Id);
        var today = DateTime.Today;

        var model = new DashboardViewModel
        {
            OpenTicketsCount = userTickets.Count(t => t.Status == TicketStatus.Open),
            ClosedTicketsCount = userTickets.Count(t => t.Status == TicketStatus.Closed),
            NewTicketsTodayCount = userTickets.Count(t => t.CreatedAt.Date == today)
        };

        return View(model);
    }



    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
