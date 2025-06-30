using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Threading.Tasks;
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

        public AdminController(UserManager<AppUser> userManager,RoleManager<IdentityRole>roleManager ,ProjectRepository projectRepository)
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
        public async Task<IActionResult> UsersList()
        {
            var users = _userManager.Users.ToList();
            var userModels = new List<AdminUserViewModel>();
            foreach (var u in users)
            {
                var roles = await _userManager.GetRolesAsync(u);
                userModels.Add(new AdminUserViewModel
                {
                    Id = u.Id,
                    UserName = u.UserName,
                    Roles = roles.Select(r => new SelectListItem { Value = r, Text = r }).ToList()
                });
            }
            return View(users);
        }
        [HttpGet]
        public IActionResult CreateUser()
        {
            var model = new AdminUserViewModel
            {
                Roles = _roleManager.Roles
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
                model.Roles = _roleManager.Roles
                    .Select(r => new SelectListItem { Value = r.Name, Text = r.Name })
                    .ToList();
                return View(model);
            }
            var user = new AppUser { UserName = model.UserName };
            var result = await _userManager.CreateAsync(user, model.Password! );
            if (result.Succeeded)
            {
                var roleResult = await _userManager.AddToRoleAsync(user, model.Role!);
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
                model.Roles = _roleManager.Roles
                    .Select(r => new SelectListItem { Value = r.Name, Text = r.Name })
                    .ToList();
                
            }
            return View(model);

        }


        public IActionResult ProjectsList()
        {
            var projects = _projectRepository.GetAllProjectsAsync();
            return View(projects);
        }


    }
}
