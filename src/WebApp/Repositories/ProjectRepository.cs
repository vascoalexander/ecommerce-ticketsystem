using Microsoft.EntityFrameworkCore;
using WebApp.Data;
using WebApp.Models;

namespace WebApp.Repositories;

public class ProjectRepository
{
    private readonly AppDbContext _context;

    public ProjectRepository(AppDbContext context)
    {
        _context = context;
    }
    public async Task<List<ProjectModel>> GetAllProjectsAsync()
    {
        return await _context.Projects.
            Include(p => p.Tickets)
            .ToListAsync();
    }

    public async Task<ProjectModel?> GetProjectById(int id)
    {
        return await _context.Projects
            .Include(p => p.Tickets)
            .ThenInclude(t => t.CreatorUser)
            .Include(p => p.Tickets)
            .ThenInclude(t => t.AssignedUser)
            .FirstOrDefaultAsync(p => p.Id == id);
    }




    public async Task CreateProject(ProjectModel project)
    {
        _context.Projects.Add(project);
        await _context.SaveChangesAsync();
    }
    public async Task UpdateProject(ProjectModel project)
    {
        _context.Projects.Update(project);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteProject(int Id)
    {
        var project = await _context.Projects.FindAsync(Id);
        if (project != null)
        {
            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();
        }
    }


}