// TicketController.cs

using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebApp.Models;
using WebApp.Repositories;
using WebApp.ViewModels;
using WebApp.Helper;

namespace WebApp.Controllers;

[Authorize]
public class TicketController : Controller
{
    private readonly TicketRepository _ticketRepository;
    private readonly ProjectRepository _projectRepository;
    private readonly FileRepository _fileRepository;
    private readonly UserManager<AppUser> _userManager;
    private readonly TicketHistoryRepository _ticketHistoryRepository;
    private readonly TicketCommentsRepository _ticketCommentsRepository;
    private readonly MessageRepository _messageRepository;

    public TicketController(
        TicketRepository ticketRepository,
        ProjectRepository projectRepository,
        FileRepository fileRepository,
        TicketHistoryRepository ticketHistoryRepository,
        TicketCommentsRepository ticketCommentsRepository,
        MessageRepository messageRepository,
        UserManager<AppUser> userManager)
    {
        _ticketRepository = ticketRepository;
        _projectRepository = projectRepository;
        _fileRepository = fileRepository;
        _userManager = userManager;
        _ticketHistoryRepository = ticketHistoryRepository;
        _ticketCommentsRepository = ticketCommentsRepository;
        _messageRepository = messageRepository;
    }

    [HttpGet]
    public async Task<IActionResult> TicketList(string? search, string? status, int? projectId, string? show, string? assignedUser, DateTime? startDate, DateTime? endDate, string? sortOrder, string? creatorId)
    {
        var tickets = await _ticketRepository.GetAllTicketsAsync();
        var currentUser = await _userManager.GetUserAsync(User);
        var userId = currentUser?.Id;
        var projects = await _projectRepository.GetAllProjectsAsync();
        var users = await Utility.GetUsersExcludingSystemAsync(_userManager);
        if (show != "all" && !string.IsNullOrEmpty(userId))
        {
            tickets = tickets.Where(t =>
                t.CreatorUserId == userId ||
                t.AssignedUserId == userId
            ).ToList();
        }
        TicketStatus? statusEnum;
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
                .Where(t => t.CreatorUser.Id == creatorId)
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
            AvailableUsers = await Utility.GetUsersExcludingSystemAsync(_userManager)
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

        var systemUser = await _userManager.FindByNameAsync("system");

        if (ticket.AssignedUser != null)
        {
            var message = new Message
            {
                Sender = systemUser!,
                SenderId = systemUser!.Id,
                Receiver = ticket.AssignedUser,
                ReceiverId = ticket.AssignedUserId!,
                SentAt = DateTime.Now.ToUniversalTime(),
                Subject = $"Ein Neues Ticket mit der ID: {ticket.Id} wurde erstellt",
                Body = $"Das Ticket wurde von {currentUser!.UserName} um {ticket.CreatedAt} erstellt"

            };
            await _messageRepository.AddMessage(message);
            await _messageRepository.SaveChangesAsync();
        }

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
            AvailableUsers = await Utility.GetUsersExcludingSystemAsync(_userManager),
            Status = ticket.Status

        };

        return View(viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(EditTicketViewModel viewModel, string submitAction)
    {
        var ticketToUpdate = await _ticketRepository.GetTicketByIdAsync(viewModel.TicketId);
        if (ticketToUpdate == null) return NotFound();

        var currentUser = await _userManager.GetUserAsync(User);

        if (submitAction == "close")
        {
            _ticketHistoryRepository.TrackChange(ticketToUpdate, TicketProperty.Status, ticketToUpdate.Status.ToString(), nameof(TicketStatus.Closed), currentUser?.Id);
            await _ticketHistoryRepository.SaveChangesAsync();
            ticketToUpdate.Status = TicketStatus.Closed;
            await _ticketRepository.UpdateTicketAsync(ticketToUpdate);
            TempData["ToastMessage"] = "Ticket erfolgreich geschlossen.";
            return RedirectToAction("Detail", new { id = ticketToUpdate.Id });
        }



        if (!ModelState.IsValid)
        {
            viewModel.AvailableProjects = await _projectRepository.GetAllProjectsAsync();
            viewModel.AvailableUsers = _userManager.Users.ToList();
            TempData["ToastMessage"] = "Ticket konnte nicht bearbeitet werden.";
            return View(viewModel);
        }
        if (submitAction == "reopen")
        {
            var previousStatus = ticketToUpdate.Status;
            var previousUser = ticketToUpdate.AssignedUser;

            _ticketHistoryRepository.TrackChange(
                ticketToUpdate,
                TicketProperty.Status,
                previousStatus.ToString(),
                nameof(TicketStatus.Open),
                currentUser?.Id
            );

            ticketToUpdate.Status = TicketStatus.Open;

            if (previousUser?.UserName != ticketToUpdate.AssignedUser?.UserName)
            {
                _ticketHistoryRepository.TrackChange(
                    ticketToUpdate,
                    TicketProperty.AssignedUser,
                    previousUser?.UserName,
                    ticketToUpdate.AssignedUser?.UserName,
                    currentUser?.Id
                );
            }

            await _ticketRepository.UpdateTicketAsync(ticketToUpdate);
            await _ticketHistoryRepository.SaveChangesAsync();

            TempData["ToastMessage"] = "Ticket wurde wieder geöffnet.";
        }

        var assignedUser = await _userManager.FindByIdAsync(viewModel.AssignedUserId!);
        if (assignedUser == null) return NotFound();

        if (ticketToUpdate.Title != viewModel.Title)
        {
            _ticketHistoryRepository.TrackChange(ticketToUpdate, TicketProperty.Title, ticketToUpdate.Title, viewModel.Title, currentUser?.Id);
            ticketToUpdate.Title = viewModel.Title;
        }

        if (ticketToUpdate.Description != viewModel.Description)
        {
            _ticketHistoryRepository.TrackChange(ticketToUpdate, TicketProperty.Description, ticketToUpdate.Description, viewModel.Description, currentUser?.Id);
            ticketToUpdate.Description = viewModel.Description;
        }

        if (ticketToUpdate.ProjectId != viewModel.ProjectId)
        {
            var oldProject = ticketToUpdate.Project.Title;
            var newProject = (await _projectRepository.GetProjectById(viewModel.ProjectId))?.Title;
            _ticketHistoryRepository.TrackChange(ticketToUpdate, TicketProperty.Project, oldProject, newProject, currentUser?.Id);
            ticketToUpdate.ProjectId = viewModel.ProjectId;
        }

        var oldAssignedUserId = ticketToUpdate.AssignedUser?.Id;
        var newAssignedUserId = viewModel.AssignedUserId;

        if (oldAssignedUserId != newAssignedUserId)
        {
            var oldUserName = ticketToUpdate.AssignedUser?.UserName;
            var newUserName = assignedUser.UserName;

            _ticketHistoryRepository.TrackChange(ticketToUpdate, TicketProperty.AssignedUser, oldUserName, newUserName, currentUser?.Id);

        }
        else if (ticketToUpdate.Status == TicketStatus.Open && ticketToUpdate.AssignedUser != null)
        {
            ticketToUpdate.Status = TicketStatus.InProgress;
        }

        await _ticketRepository.UpdateTicketAsync(ticketToUpdate);
        await _ticketHistoryRepository.SaveChangesAsync();

        var systemUser = await _userManager.FindByNameAsync("system");

        var message = new Message
        {
            Sender = systemUser!,
            SenderId = systemUser!.Id,
            Receiver = assignedUser,
            ReceiverId = assignedUser.Id,
            SentAt = DateTime.Now.ToUniversalTime(),
            Subject = $"Sie wurden dem Ticket mit der ID: {ticketToUpdate.Id} für die bearbeitung zugewiesen",
            Body = $"Das Ticket wurde Ihnen zugewiesen von {currentUser!.UserName} um {DateTime.Now.ToUniversalTime()}"

        };
        await _messageRepository.AddMessage(message);
        await _messageRepository.SaveChangesAsync();

        TempData["ToastMessage"] = "Ticket erfolgreich bearbeitet.";
        return RedirectToAction("Detail", new { id = ticketToUpdate.Id });
    }



    [HttpGet]
    public async Task<IActionResult> Detail(int id, string returnUrl = "")
    {    TempData["ReturnUrl"] = returnUrl;
        var ticket = await _ticketRepository.GetTicketByIdAsync(id);
    
        
        if (ticket == null)
        {
            TempData["ToastMessage"] = "Ticket nicht gefunden.";
            return NotFound();
        }
      

        var history = await _ticketHistoryRepository.GetHistoryForTicketAsync(id);
        var comments = await _ticketCommentsRepository.GetAllCommentsForTicketAsync(id);


        var viewModel = new TicketDetailViewModel
        {
            Ticket = ticket,
            History = history,
            Comments = comments
        };

        return View(viewModel);
    }
    [HttpPost]
    public async Task<IActionResult> Detail(TicketDetailViewModel viewModel)
    {
        if (viewModel.Ticket == null)
        {
            TempData["ToastMessage"] = "Ticket nicht gefunden.";
            return NotFound();
        }

        var ticket = await _ticketRepository.GetTicketByIdAsync(viewModel.Ticket.Id);
        if (ticket == null)
        {
            TempData["ToastMessage"] = "Ticket nicht gefunden.";
            return NotFound();
        }

        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            TempData["ToastMessage"] = "Benutzer nicht angemeldet.";
            return Unauthorized();
        }

        if (string.IsNullOrWhiteSpace(viewModel.NewCommentContent))
        {
            ModelState.AddModelError(nameof(viewModel.NewCommentContent), "Kommentar darf nicht leer sein.");
            viewModel.Ticket = ticket;
            viewModel.History = await _ticketHistoryRepository.GetHistoryForTicketAsync(ticket.Id);
            viewModel.Comments = await _ticketCommentsRepository.GetAllCommentsForTicketAsync(ticket.Id);
            return View(viewModel);
        }

        _ticketCommentsRepository.CreateComment(ticket.Id, viewModel.NewCommentContent, currentUser.Id);
        await _ticketCommentsRepository.SaveCommentAsync();

        var systemUser = await _userManager.FindByNameAsync("system");

        var message = new Message
        {
            Sender = systemUser!,
            SenderId = systemUser!.Id,
            Receiver = ticket.AssignedUser!,
            ReceiverId = ticket.AssignedUser!.Id,
            SentAt = DateTime.Now.ToUniversalTime(),
            Subject = $"Es wurde ein Neues Kommentar für das Ticket: {ticket.Id} erstellt",
            Body = $"""
                    Das Kommentar wurde vom Benutzer: {currentUser.UserName} um {DateTime.Now.ToUniversalTime()} hinterlassen:
                    {viewModel.NewCommentContent}
                    """

        };
        await _messageRepository.AddMessage(message);
        await _messageRepository.SaveChangesAsync();

        TempData["ToastMessage"] = "Kommentar erfolgreich hinzugefügt.";
        return RedirectToAction(nameof(Detail), new { id = ticket.Id });
    }

    [HttpGet]
    public async Task<IActionResult> Upload(int id)
    {
        var ticket = await _ticketRepository.GetTicketByIdAsync(id);
        if (ticket == null)
        {
            return NotFound();
        }

        var viewModel = new UploadViewModel
        {
            TicketId = id,
            UploadedFile = null
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Upload(UploadViewModel model)
    {
        if (!ModelState.IsValid || model.UploadedFile == null || model.UploadedFile.Length == 0)
        {
            ModelState.AddModelError("UploadedFile", "Ungültige Datei.");
            return View(model);
        }
        var originalFileName = Path.GetFileName(model.UploadedFile.FileName);
        var extension = Path.GetExtension(originalFileName);

        string hash;
        using (var sha = SHA256.Create())
        {
            var inputBytes = Encoding.UTF8.GetBytes($"{originalFileName}-{DateTime.UtcNow.Ticks}");
            var hashBytes = sha.ComputeHash(inputBytes);
            hash = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
        }
        var storedFileName = $"{hash}{extension}";
        var fullpath = Path.Combine(Directory.GetCurrentDirectory(), "Uploads", storedFileName);

        if (System.IO.File.Exists(fullpath))
        {
            ModelState.AddModelError("FileName", "Filename already exists.");
            return View(model);
        }

        await using var fileStream = new FileStream(fullpath, FileMode.Create);
        await model.UploadedFile.CopyToAsync(fileStream);

        var ticketId = model.TicketId;
        var ticket = await _ticketRepository.GetTicketByIdAsync(ticketId);
        if (ticket == null)
        {
            return NotFound();
        }

        var ticketFile = new TicketFile
        {
            OriginalName = originalFileName,
            StoredName = storedFileName,
            FileSize = model.UploadedFile.Length,
            UploadDate = DateTime.UtcNow,
            TicketId = ticket.Id,
            Ticket = ticket
        };
        await _fileRepository.AddFileAsync(ticketFile);
        await _fileRepository.SaveChangesAsync();

        return RedirectToAction("Edit", new {id = ticketId });
    }

    [HttpGet]
    public async Task<IActionResult> DownloadFile(int fileId)
    {
        var file = await _fileRepository.GetFileByIdAsync(fileId);
        if (file == null)
        {
            return NotFound();
        }

        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "Uploads", file.StoredName);
        if (!System.IO.File.Exists(filePath))
        {
            return NotFound();
        }

        var memory = new MemoryStream();
        await using (var stream = new FileStream(filePath, FileMode.Open))
        {
            await stream.CopyToAsync(memory);
        }
        memory.Position = 0;

        return File(memory, "application/octet-stream", file.OriginalName);
    }
    public IActionResult BackOrRedirect()
    {
        var returnUrl = TempData["ReturnUrl"] as string;
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }
        return RedirectToAction("TicketList", "Ticket");
    }
}
