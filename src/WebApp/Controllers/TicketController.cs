using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers;

public class TicketController : Controller
{
    // GET
    public IActionResult TicketList()
    {
        return View();
    }
}