// TicketController.cs

using System.Security.Cryptography;
using System.Text;
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
    private readonly FileRepository _fileRepository;
    private readonly UserManager<AppUser> _userManager;

    public TicketController(
        TicketRepository ticketRepository,
        ProjectRepository projectRepository,
        FileRepository fileRepository,
        UserManager<AppUser> userManager)
    {
        _ticketRepository = ticketRepository;
        _projectRepository = projectRepository;
        _fileRepository = fileRepository;
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
        return RedirectToAction("TicketList");
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

        return RedirectToAction("TicketList");
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
}
