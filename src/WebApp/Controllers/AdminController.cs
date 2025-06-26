using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WebApp.Models;
using WebApp.Repositories;

namespace WebApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {

        private readonly AccountRepository _accountRepository;
        private readonly ProjectRepository _projectRepository;

        public AdminController(AccountRepository accountRepository, ProjectRepository projectRepository)
        { _projectRepository = projectRepository;
            _accountRepository = accountRepository; }


        public IActionResult AdminPage()
        {
            return View();
        }
        public IActionResult UserManagement()
        {
            return View();
        }
        public IActionResult UsersList()
        {
            var users = _accountRepository.GettAllUsers().Select(u => new AdminUserModel
            {
                Id = u.Id,
                UserName = u.UserName,
                Roles = (IList<SelectListItem>)_accountRepository.GetUserRoles(u.Id).Result


            }).ToList();
            return View(users);
        }
        [HttpGet]
        public IActionResult CreateUser()
        {
            var model = new AdminUserModel
            {
                Roles = _accountRepository.GetRoles().Select(r => new SelectListItem { Value = r.Name, Text = r.Name }).ToList()
            };
            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(AdminUserModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Roles = _accountRepository.GetRoles().Select(r => new SelectListItem { Value = r.Name, Text = r.Name }).ToList();
                return View(model);
            }
            var result = await _accountRepository.CreateUser(model.UserName, model.Password, model.Role);
            if (result.Succeeded)
            {
                return RedirectToAction("Users");
            }
            else { model.Roles = _accountRepository.GetRoles().Select(r => new SelectListItem { Value = r.Name, Text = r.Name }).ToList(); }
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
                
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
