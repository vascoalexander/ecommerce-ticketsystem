using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebApp.Models;
using WebApp.Repositories;
using WebApp.ViewModels;

namespace WebApp.Controllers;

public class TicketController : Controller
{
    private readonly TicketRepository _ticketRepository;
    private readonly ProjectRepository _projectRepository;
    private readonly UserManager<AppUser> _userManager;

    public TicketController(
        TicketRepository ticketRepository,
        ProjectRepository projectRepository,
        UserManager<AppUser> userManager)
    {
        _ticketRepository = ticketRepository;
        _projectRepository = projectRepository;
        _userManager = userManager;
    }

    // GET: TicketList
    [HttpGet]
    public async Task<IActionResult> TicketList()
    {
        var tickets = await _ticketRepository.GetAllTicketsAsync();
        var projects = await _projectRepository.GetAllProjectsAsync();
        var users = _userManager.Users.ToList();

        var viewModel = new TicketListViewModel
        {
            Tickets = tickets,
            AvailableProjects = projects,
            AvailableUsers = users
        };

        return View(viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> TicketList(TicketListViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            viewModel.Tickets = await _ticketRepository.GetAllTicketsAsync();
            viewModel.AvailableProjects = await _projectRepository.GetAllProjectsAsync() ?? new List<ProjectModel>();
            viewModel.AvailableUsers = _userManager.Users.ToList() ?? new List<AppUser>();

            return View(viewModel);
        }

        var currentUser = await _userManager.GetUserAsync(User);
        var assignedUser = await _userManager.FindByIdAsync(viewModel.NewTicket.AssignedUserId);
        var project = _projectRepository.GetProjectByID(viewModel.NewTicket.ProjectId);

        var ticket = new TicketModel
        {
            Title = viewModel.NewTicket.Title,
            Description = viewModel.NewTicket.Description,
            AssignedUser = assignedUser,
            Project = project,
            Status = "Open",
            CreatedAt = DateTime.Now.ToUniversalTime(),
            AssignedAt = DateTime.Now.ToUniversalTime(),
            CreatorUser = currentUser
        };

        await _ticketRepository.CreateTicketAsync(ticket);
        return RedirectToAction("TicketList");
    }


    // GET: Detailansicht eines Tickets
    [HttpGet]
    public async Task<IActionResult> Detail(int id)
    {
        var ticket = await _ticketRepository.GetTicketByIdAsync(id);
        if (ticket == null)
        {
            return NotFound();
        }

        return View(ticket);
    }
}