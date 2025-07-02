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
                .Where(t => t.Status == statusEnum).ToList();
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

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var viewModel = new CreateTicketViewModel
        {
            AvailableProjects = await _projectRepository.GetAllProjectsAsync(),
            AvailableUsers = _userManager.Users.ToList()
        };

        return View(viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateTicketViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            viewModel.AvailableProjects = await _projectRepository.GetAllProjectsAsync();
            viewModel.AvailableUsers = _userManager.Users.ToList();
            return View(viewModel);
        }

        var currentUser = await _userManager.GetUserAsync(User);
        var assignedUser = await _userManager.FindByIdAsync(viewModel.AssignedUserId);
        var project = await _projectRepository.GetProjectById(viewModel.ProjectId);

        var ticket = new TicketModel
        {
            Title = viewModel.Title,
            Description = viewModel.Description,
            AssignedUser = assignedUser,
            Project = project!,
            Status = assignedUser != null ? TicketStatus.InProgress : TicketStatus.Open,
            CreatedAt = DateTime.UtcNow,
            AssignedAt = DateTime.UtcNow,
            CreatorUser = currentUser!
        };

        await _ticketRepository.CreateTicketAsync(ticket);
        TempData["ToastMessage"] = "Ticket erfolgreich erstellt.";
        return RedirectToAction("Detail", new { id = ticket.Id });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var ticket = await _ticketRepository.GetTicketByIdAsync(id);
        if (ticket == null) return NotFound();

        var viewModel = new EditTicketViewModel
        {
            TicketId = ticket.Id,
            Title = ticket.Title,
            Description = ticket.Description,
            ProjectId = ticket.ProjectId,
            AssignedUserId = ticket.AssignedUser?.Id,
            AvailableProjects = await _projectRepository.GetAllProjectsAsync(),
            AvailableUsers = _userManager.Users.ToList()
        };

        return View(viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(EditTicketViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            viewModel.AvailableProjects = await _projectRepository.GetAllProjectsAsync();
            viewModel.AvailableUsers = _userManager.Users.ToList();
            TempData["ToastMessage"] = "Ticket konnte nicht bearbeitet werden.";
            return View(viewModel);
        }

        var ticketToUpdate = await _ticketRepository.GetTicketByIdAsync(viewModel.TicketId ?? 0);
        if (ticketToUpdate == null) return NotFound();

        var assignedUser = await _userManager.FindByIdAsync(viewModel.AssignedUserId);

        ticketToUpdate.Title = viewModel.Title;
        ticketToUpdate.Description = viewModel.Description;
        ticketToUpdate.ProjectId = viewModel.ProjectId;

        if (ticketToUpdate.AssignedUser?.Id != viewModel.AssignedUserId)
        {
            if (string.IsNullOrEmpty(viewModel.AssignedUserId))
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
        else if (ticketToUpdate.Status == TicketStatus.Open && ticketToUpdate.AssignedUser != null)
        {
            ticketToUpdate.Status = TicketStatus.InProgress;
        }

        await _ticketRepository.UpdateTicketAsync(ticketToUpdate);
        TempData["ToastMessage"] = "Ticket erfolgreich bearbeitet.";
        return RedirectToAction("Detail", new { id = ticketToUpdate.Id });
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