using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Threading.Tasks;
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


        public IActionResult AdminPage()
        {
            return View();
        }
        public IActionResult UserManagement()
        {
            return View();
        }

        public async Task<IActionResult> UsersList(string? roleFilter, string? search, string? sortOrder)
        {
            var users = await _userManager.Users.ToListAsync();

            if (!string.IsNullOrWhiteSpace(search))
                users = users
                    .Where(u => u.UserName
                        .Contains(search, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            users = sortOrder switch
            {
                "name_desc" => users.OrderByDescending(s => s.UserName).ToList(),
                "id_desc" => users.OrderByDescending(s => s.Id).ToList(),
                _ => users.OrderBy(s => s.UserName).ToList(),
            };
            var model = new List<AdminUserViewModel>();
            foreach (var user in users)
            {
                model.Add(new AdminUserViewModel
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    AssignedRoles = await _userManager.GetRolesAsync(user)

                });
            }
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> CreateUser()
        {
            var model = new AdminUserViewModel
            {
                AvailableRoles = _roleManager.Roles
                    .Select(r => new SelectListItem { Value = r.Name, Text = r.Name })
                    .ToList()
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(AdminUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.AvailableRoles = _roleManager.Roles
                    .Select(r => new SelectListItem { Value = r.Name, Text = r.Name })
                    .ToList();
                return View(model);
            }
            var user = new AppUser { UserName = model.UserName };
            var result = await _userManager.CreateAsync(user, model.Password!);
            if (result.Succeeded)
            {
                var roleResult = await _userManager.AddToRoleAsync(user, model.SelectedRole!);
                if (roleResult.Succeeded)
                {
                    return RedirectToAction("UsersList");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {

                        ModelState.AddModelError("", error.Description);

                    }
                }
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }

                model.AvailableRoles = _roleManager.Roles
                    .Select(r => new SelectListItem { Value = r.Name, Text = r.Name })
                    .ToList();

            }
            return View(model);

        }


        public async Task<IActionResult> ProjectsList(string? search, int? id, string? category, string? status, string? sortOrder)
        {
            var projects = await _projectRepository.GetAllProjectsAsync();


            if (!string.IsNullOrEmpty(category))
            {
                projects = projects
                    .Where(p => p.Category == category)
                    .ToList();
            }

            if (!string.IsNullOrEmpty(search))
            {
                projects = projects
                    .Where(p =>
                        (p.Title?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false) ||
                          (p.Description?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false))
                    .ToList();
            }

            projects = sortOrder switch
            {
                "title_desc" => projects.OrderByDescending(p => p.Title).ToList(),
                "date_asc" => projects.OrderBy(p => p.StartDate).ToList(),
                "date_desc" => projects.OrderByDescending(p => p.StartDate).ToList(),
                "id_desc" => projects.OrderByDescending(p => p.Id).ToList(),
                _ => projects.OrderBy(p => p.Title).ToList(),
            };



            return View(projects);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateProject(ProjectModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            await _projectRepository.CreateProject(model);
            return RedirectToAction("ProjectsList");
        }

        [HttpGet]
        public async Task<IActionResult> CreateProject()
        {

            return View(new ProjectModel
            {
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddYears(1),
                Title = string.Empty,
                Description = string.Empty,

            });
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
        public async Task<IActionResult> EditProject(ProjectModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var existingproject = await _projectRepository.GetProjectById(model.Id);
            if (existingproject == null) return NotFound();


            existingproject.Title = model.Title;
            existingproject.Description = model.Description;
            existingproject.Category = model.Category;
            existingproject.StartDate = model.StartDate;
            existingproject.EndDate = model.EndDate;

            await _projectRepository.UpdateProject(existingproject);

            return RedirectToAction("ProjectsList");
        }

        [HttpGet]
        public async Task<IActionResult> DeleteProject(int id)
        {
            var project = await _projectRepository.GetProjectById(id);
            if (project == null) return NotFound();
            return View(project);
        }
        [HttpPost, ActionName("DeleteProject")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProjectConfirmend(int id)
        {
            await _projectRepository.GetProjectById(id);
            return RedirectToAction("ProjectsList");
        }

        public async Task<IActionResult> DetailsProject(int id)
        {
            var project = await _projectRepository.GetProjectById(id);
            if (project == null) return NotFound();
            var tickets = project.Tickets?.ToList() ?? new List<TicketModel>();
            var viewModel = new ProjectDetailViewModel
            {
                Project = project,
                Tickets = tickets,
            };

            return View(project);

        }







    }
}
