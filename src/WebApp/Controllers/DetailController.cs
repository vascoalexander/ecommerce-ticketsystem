using Microsoft.AspNetCore.Mvc;
using WebApp.Repositories;

namespace WebApp.Controllers
{
    public class DetailController : Controller
    {
        private readonly ProjectRepository _projectRepo;

        public DetailController(ProjectRepository projectRepo)
        {
            _projectRepo = projectRepo;
        }

        public async Task<IActionResult> Detail(int Id)
        {
            var project = await _projectRepo.GetProjectById(Id);
            if(project == null) { return NotFound(); }
            return View(project);
        }
        

    }
}
