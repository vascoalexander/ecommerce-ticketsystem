using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlTypes;
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
    public async Task<IActionResult> TicketList(string? search, string? status, int? projectId,string? show ,string? assignedUser, DateTime? startDate, DateTime? endDate, string? sortOrder, string? creatorId)
    {
        var tickets = await _ticketRepository.GetAllTicketsAsync();
        var currentUser = await _userManager.GetUserAsync(User);
        var userId = currentUser?.Id;
        var projects = await _projectRepository.GetAllProjectsAsync();
        var users = _userManager.Users.ToList();
        if (show != "all" && !string.IsNullOrEmpty(userId))
        {
            tickets = tickets.Where(t =>
                t.CreatorUserId == userId ||
                t.AssignedUserId == userId
            ).ToList();
        }
        TicketStatus? statusEnum = null;
        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<TicketStatus>
                          (status, true, out var parsedStatus))
        {
            statusEnum = parsedStatus;
            tickets = tickets
                .Where(t => t.Status == statusEnum).
        ToList();
        }
        if (projectId.HasValue)
        {
            tickets = tickets
                .Where(t => t.ProjectId == projectId.Value).ToList();

        }

        if (!string.IsNullOrWhiteSpace(creatorId))
        {
            tickets = tickets
                .Where(t => t.CreatorUser?.Id == creatorId)
                .ToList();
        }

        if (!string.IsNullOrWhiteSpace(assignedUser))
        {
            tickets = tickets
                .Where(t => t.AssignedUserId == assignedUser).ToList();
        }
        if (!string.IsNullOrWhiteSpace(search))
        {
            tickets = tickets
                .Where(t => (t.Title?
                    .Contains(search, StringComparison.OrdinalIgnoreCase) ?? false) ||
                      (t.Description?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false) ||
                      (t.AssignedUser?.Id.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false))
                .ToList();
        }

        if (startDate.HasValue)
        {
            tickets = tickets
                .Where(t => t.CreatedAt >= startDate.Value).ToList();

        }

        if (endDate.HasValue)
        {
            tickets = tickets
                .Where(t => t.CreatedAt <= endDate.Value).ToList();
        }

        tickets = sortOrder switch
        {
            "date_desc" => tickets.OrderByDescending(t => t.CreatedAt).ToList(),
            "title_asc" => tickets.OrderBy(t => t.Title).ToList(),
            "title_desc" => tickets.OrderByDescending(t => t.Title).ToList(),
            _ => tickets.OrderBy(t => t.Id).ToList()
        };

        var statusOptions = Enum.GetValues(typeof(TicketStatus)).Cast<TicketStatus>().ToList();
        var viewmodel = new TicketListViewModel
        {
            Tickets = tickets,
            AvailableProjects = projects,
            AvailableUsers = users,
            StatusOptions = statusOptions,
            SelectedStatus = status,
            Search = search,
            SelectedCreatorId = creatorId,
            SelectedAssigneeId = assignedUser,
            FromDate = startDate,
            ToDate = endDate,
        };
        return View(viewmodel);
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
        TempData["ToastMessage"] = "Ticket erfolgreich erstellt.";
        return RedirectToAction("TicketList");
    }

    [HttpPost]
    public async Task<IActionResult> Edit(int ticketId, NewTicketInputModel updatedTicket)
    {
        if (!ModelState.IsValid)
        {
            TempData["ToastMessage"] = "Ticket konnte nicht bearbeitet werden.";
            return RedirectToAction("TicketList");
        }

        var ticketToUpdate = await _ticketRepository.GetTicketByIdAsync(ticketId);
        if (ticketToUpdate == null)
        {
            TempData["ToastMessage"] = "Ticket nicht gefunden.";
            return NotFound();
        }

        var assignedUser = await _userManager.FindByIdAsync(updatedTicket.AssignedUserId);

        ticketToUpdate.Title = updatedTicket.Title;
        ticketToUpdate.Description = updatedTicket.Description;
        ticketToUpdate.ProjectId = updatedTicket.ProjectId;

        if (ticketToUpdate.AssignedUser?.Id != updatedTicket.AssignedUserId)
        {
            if (string.IsNullOrEmpty(updatedTicket.AssignedUserId))
            {
                ticketToUpdate.Status = TicketStatus.Open;
                ticketToUpdate.AssignedUser = null;
            }
            else
            {
                ticketToUpdate.Status = TicketStatus.InProgress;
                ticketToUpdate.AssignedUser = assignedUser;
            }
        }
        else
        {
            if (ticketToUpdate.Status == TicketStatus.Open)
            {
                ticketToUpdate.Status = TicketStatus.InProgress;
            }
        }

        await _ticketRepository.UpdateTicketAsync(ticketToUpdate);

        TempData["ToastMessage"] = "Ticket erfolgreich bearbeitet.";
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
            TempData["ToastMessage"] = "Ticket nicht gefunden.";
            return NotFound();
        }

        return View(ticket);
    }

    [HttpPost]
    public async Task<IActionResult> Close(int ticketId)
    {
        var ticketToUpdate = await _ticketRepository.GetTicketByIdAsync(ticketId);
        if (ticketToUpdate == null)
        {
            TempData["ToastMessage"] = "Ticket nicht gefunden.";
            return NotFound();
        }

        ticketToUpdate.Status = TicketStatus.Closed;
        await _ticketRepository.UpdateTicketAsync(ticketToUpdate);

        TempData["ToastMessage"] = "Ticket erfolgreich geschlossen.";
        return RedirectToAction("TicketList");
    }

    [HttpPost]
    public async Task<IActionResult> Reopen(int ticketId)
    {
        var ticketToUpdate = await _ticketRepository.GetTicketByIdAsync(ticketId);
        if (ticketToUpdate == null)
        {
            TempData["ToastMessage"] = "Ticket nicht gefunden.";
            return NotFound();
        }

        ticketToUpdate.Status = TicketStatus.Open;
        ticketToUpdate.AssignedUser = null;

        await _ticketRepository.UpdateTicketAsync(ticketToUpdate);

        TempData["ToastMessage"] = "Ticket wurde wieder geöffnet.";
        return RedirectToAction("TicketList");
    }
}
