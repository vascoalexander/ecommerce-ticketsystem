using Microsoft.EntityFrameworkCore;
using WebApp.Data;
using WebApp.Models;

namespace WebApp.Repositories;

public class ProjectRepository
{
    private readonly AppDbContext _context;
    
    public  ProjectRepository(AppDbContext context)
    {
        _context = context;
    }
    public async Task<List<ProjectModel>> GetAllProjectsAsync()
    {
        return await _context.Projects.ToListAsync();
    }

    public ProjectModel? GetProjectByID(int Id)=>_context.Projects.Include(p=>p.Tickets).FirstOrDefault(p=> p.Id == Id);

    public void AddProject(ProjectModel project)
    {
        _context.Projects.Add(project);
        _context.SaveChanges();
    }
    public void UpdateProject(ProjectModel project)
    {
        _context.Projects.Update(project);
        _context.SaveChanges();
    }
    public void DeleteProject(int Id)
    {
        var project = _context.Projects.Find(Id);
        if(project != null)
        {
            _context.Projects.Remove(project);
            _context.SaveChanges();
        }
    }

}