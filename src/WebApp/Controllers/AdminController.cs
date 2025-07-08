using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebApp.Models;
using WebApp.Repositories;
using WebApp.ViewModels;


namespace WebApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ProjectRepository _projectRepository;

        public AdminController(UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager, ProjectRepository projectRepository)

        {
            _projectRepository = projectRepository;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> AdminPage(string? search,
            bool showInactive = false)
        {
            var projects = await _projectRepository.GetAllProjectsAsync();
            if (!showInactive)
            {
                projects = projects.Where(u => u.ProjectActive).ToList();
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                projects = projects
                    .Where(p => p.Title != null && p.Title.Contains(search, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            var viewModel = new ProjectPageViewModel()
            {
                IncludeInactive = showInactive,
                Projects = projects.Select(project => new ProjectModel
                {
                    Id = project.Id,
                    Description = project.Description,
                    Title = project.Title,
                    Category = project.Category,
                    ProjectActive = project.ProjectActive,
                    StartDate = project.StartDate,
                    EndDate = project.EndDate,
                    Tickets = project.Tickets?.ToList() ?? new List<TicketModel>()
                }).ToList()
            };
            return View(viewModel);
        }


        public async Task<IActionResult> UserManagement(string? search, bool includeInactive = false)
        {
            var users = await _userManager.Users.ToListAsync();
            if (!includeInactive)
            {
                users = users.Where(u => u.IsActive).ToList();
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                users = users
                    .Where(u => u.UserName != null && u.UserName.Contains(search, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            var viewModel = new AdminUserManagementViewModel
            {
                IncludeInactive = includeInactive,
                Search = search,
                Users = new List<AdminUserViewModel>()
            }
            ;

            foreach (var user in users)
            {
                var userRoles = await _userManager.GetRolesAsync(user);
                viewModel.Users.Add(new AdminUserViewModel
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    IsActive = user.IsActive,
                    AssignedRoles = userRoles
                });

            }

            return View(viewModel);
        }
        [HttpGet]
        public async Task<IActionResult> CreateUser()
        {
            var model = new AdminUserViewModel
            {
                AvailableRoles = await _roleManager.Roles
                    .Select(r => new SelectListItem { Value = r.Name, Text = r.Name })
                    .ToListAsync()
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(AdminUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new AppUser
                {
                    UserName = model.UserName,
                    Email = model.Email,
                    IsActive = true
                };

                var result = await _userManager.CreateAsync(user, model.Password);;

                if (result.Succeeded)
                {
                    if (!string.IsNullOrEmpty(model.SelectedRole))
                    {
                        await _userManager.AddToRoleAsync(user, model.SelectedRole);
                    }

                    return RedirectToAction(nameof(UserManagement));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            model.AvailableRoles = await _roleManager.Roles
                .Select(r => new SelectListItem { Value = r.Name, Text = r.Name })
                .ToListAsync();

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> EditUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            var userRoles = await _userManager.GetRolesAsync(user);
            var model = new AdminUserViewModel
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                IsActive = user.IsActive,
                AssignedRoles = userRoles,
                SelectedRole = userRoles.FirstOrDefault(),
                AvailableRoles = await _roleManager.Roles
                    .Select(r => new SelectListItem { Value = r.Name, Text = r.Name })
                    .ToListAsync()
            };
            return View(model);
        }

   [HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> EditUser(AdminUserViewModel model, string? changeStatus)
{
    var existinguser = await _userManager.FindByIdAsync(model.Id!);
    if (existinguser == null) return NotFound();

    if (!string.IsNullOrEmpty(changeStatus))
    {
        existinguser.IsActive = changeStatus == "activate";
        await _userManager.UpdateAsync(existinguser);
        TempData["SuccessMessage"] = existinguser.IsActive
            ? "User wurde aktiviert." : "User wurde deaktiviert.";
        return RedirectToAction(nameof(EditUser), new { id = existinguser.Id });
    }

    
    if (!ModelState.IsValid)
    {
        model.AvailableRoles = await _roleManager.Roles
            .Select(r => new SelectListItem { Value = r.Name, Text = r.Name })
            .ToListAsync();
        return View(model);
    }

    existinguser.UserName = model.UserName;
    existinguser.Email = model.Email;
   
    var result = await _userManager.UpdateAsync(existinguser);
    if (result.Succeeded)
    {
        
        var currentRoles = await _userManager.GetRolesAsync(existinguser);
        if (model.SelectedRole != null && (!currentRoles.Contains(model.SelectedRole) || currentRoles.Count > 1))
        {
            await _userManager.RemoveFromRolesAsync(existinguser, currentRoles);
            if (!string.IsNullOrEmpty(model.SelectedRole))
            {
                await _userManager.AddToRoleAsync(existinguser, model.SelectedRole);
            }
        }

        TempData["SuccessMessage"] = "User wurde erfolgreich aktualisiert.";
        return RedirectToAction(nameof(UserManagement));
    }

    foreach (var error in result.Errors)
    {
        ModelState.AddModelError("", error.Description);
    }


    model.AvailableRoles = await _roleManager.Roles
        .Select(r => new SelectListItem { Value = r.Name, Text = r.Name })
        .ToListAsync();
    return View(model);
}
        

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateProject(ProjectModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            model.StartDate = DateTime.SpecifyKind(model.StartDate, DateTimeKind.Utc);
            model.EndDate = DateTime.SpecifyKind(model.EndDate, DateTimeKind.Utc);
            await _projectRepository.CreateProject(model);
            return RedirectToAction("AdminPage");
        }

        [HttpGet]
        public Task<IActionResult> CreateProject()
        {

            return Task.FromResult<IActionResult>(View(new ProjectModel
            {
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddYears(1),
                Title = string.Empty,
                Description = string.Empty,
                ProjectActive = true
            }));
        }

        [HttpGet]
        public async Task<IActionResult> EditProject(int id)
        {
            var project = await _projectRepository.GetProjectById(id);
            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProject(ProjectModel model, string? changeStatus = null)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var existingproject = await _projectRepository.GetProjectById(model.Id);
            if (existingproject == null) return NotFound();
            if (!string.IsNullOrEmpty(changeStatus))
            {
                existingproject.ProjectActive = changeStatus == "activate";
                await _projectRepository.UpdateProject(existingproject);
                TempData["SuccessMessage"] = existingproject.ProjectActive
                    ? "User wurde aktiviert." : "User wurde deaktiviert.";
                return RedirectToAction(nameof(EditProject), new { id = existingproject.Id });
            }


            model.StartDate = DateTime.SpecifyKind(model.StartDate, DateTimeKind.Utc);
            model.EndDate = DateTime.SpecifyKind(model.EndDate, DateTimeKind.Utc);
            existingproject.Title = model.Title;
            existingproject.Description = model.Description;
            existingproject.Category = model.Category;
            existingproject.StartDate = model.StartDate;
            existingproject.EndDate = model.EndDate;

            await _projectRepository.UpdateProject(existingproject);


            TempData["SuccessMessage"] = "Projekt wurde erfolgreich aktualisiert.";
            return RedirectToAction(nameof(AdminPage));
        }

        public async Task<IActionResult> DetailsProject(int id)
        {
            var project = await _projectRepository.GetProjectById(id);
            if (project == null) return NotFound();
            var tickets = project.Tickets.ToList();
            var viewModel = new ProjectDetailViewModel
            {
                Project = project,
                Tickets = tickets,
            };

            return View(viewModel);
        }
    }
}
