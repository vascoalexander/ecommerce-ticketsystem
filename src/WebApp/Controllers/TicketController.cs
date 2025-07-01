// TicketController.cs

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebApp.Models;
using WebApp.Repositories;
using WebApp.ViewModels;

namespace WebApp.Controllers;

[Authorize]
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
    public async Task<IActionResult> Create(TicketListViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            viewModel.Tickets = await _ticketRepository.GetAllTicketsAsync();
            viewModel.AvailableProjects = await _projectRepository.GetAllProjectsAsync() ?? new List<ProjectModel>();
            viewModel.AvailableUsers = _userManager.Users.ToList() ?? new List<AppUser>();
            return View("TicketList", viewModel);
        }

        var currentUser = await _userManager.GetUserAsync(User);
        var assignedUser = await _userManager.FindByIdAsync(viewModel.NewTicket.AssignedUserId);
        var project = await _projectRepository.GetProjectById(viewModel.NewTicket.ProjectId);

        var ticket = new TicketModel
        {
            Title = viewModel.NewTicket.Title,
            Description = viewModel.NewTicket.Description,
            AssignedUser = assignedUser,
            Project = project!,
            Status = assignedUser != null ? TicketStatus.InProgress : TicketStatus.Open,
            CreatedAt = DateTime.Now.ToUniversalTime(),
            AssignedAt = DateTime.Now.ToUniversalTime(),
            CreatorUser = currentUser!
        };

        await _ticketRepository.CreateTicketAsync(ticket);
        return View("Create");
    }

    [HttpPost]
    public async Task<IActionResult> Edit(int ticketId, NewTicketInputModel updatedTicket)
    {
        if (!ModelState.IsValid)
        {
            return RedirectToAction("TicketList");
        }

        var ticketToUpdate = await _ticketRepository.GetTicketByIdAsync(ticketId);
        if (ticketToUpdate == null) return NotFound();

        var assignedUser = await _userManager.FindByIdAsync(updatedTicket.AssignedUserId);

        ticketToUpdate.Title = updatedTicket.Title;
        ticketToUpdate.Description = updatedTicket.Description;
        ticketToUpdate.ProjectId = updatedTicket.ProjectId;
        
        if (ticketToUpdate.AssignedUser?.Id != updatedTicket.AssignedUserId)
        {
            // Wenn der zugewiesene Benutzer abgewählt wurde
            if (String.IsNullOrEmpty(updatedTicket.AssignedUserId))
            {
                ticketToUpdate.Status = TicketStatus.Open;
                ticketToUpdate.AssignedUser = null;
            }
            else // Wenn ein neuer Benutzer zugewiesen wurde
            {
                ticketToUpdate.Status = TicketStatus.InProgress;
                ticketToUpdate.AssignedUser = assignedUser;
            }
        }
        else // Wenn der zugewiesene Benutzer gleich geblieben ist
        {
            if (ticketToUpdate.Status == TicketStatus.Open)
            {
                ticketToUpdate.Status = TicketStatus.InProgress;
            }
        }

        await _ticketRepository.UpdateTicketAsync(ticketToUpdate);

        return RedirectToAction("TicketList");
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var ticket = await _ticketRepository.GetTicketByIdAsync(id);
        if (ticket == null) return NotFound();

        var viewModel = new TicketListViewModel
        {
            NewTicket = new NewTicketInputModel
            {
                TicketId = ticket.Id,
                Title = ticket.Title,
                Description = ticket.Description,
                ProjectId = ticket.Project.Id,
                AssignedUserId = ticket.AssignedUser?.Id
            },
            AvailableProjects = await _projectRepository.GetAllProjectsAsync(),
            AvailableUsers = _userManager.Users.ToList(),
            Tickets = await _ticketRepository.GetAllTicketsAsync()
        };

        return View("TicketList", viewModel);
    }

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
    
    [HttpPost]
    public async Task<IActionResult> Close(int ticketId)
    {
        var ticketToUpdate = await _ticketRepository.GetTicketByIdAsync(ticketId);
        if (ticketToUpdate == null) return NotFound();

        ticketToUpdate.Status = TicketStatus.Closed;

        await _ticketRepository.UpdateTicketAsync(ticketToUpdate);

        return RedirectToAction("TicketList");
    }
    
    [HttpPost]
    public async Task<IActionResult> Reopen(int ticketId)
    {
        var ticketToUpdate = await _ticketRepository.GetTicketByIdAsync(ticketId);
        if (ticketToUpdate == null) return NotFound();

        ticketToUpdate.Status = TicketStatus.Open;
        ticketToUpdate.AssignedUser = null;

        await _ticketRepository.UpdateTicketAsync(ticketToUpdate);

        return RedirectToAction("TicketList");
    }
}
