using WebApp.Models;

namespace WebApp.ViewModels;

public class ProjectPageViewModel
{
    public List<ProjectModel> Projects { get; set; } = new();
    public bool IncludeInactive { get; set; }
    
    public string? Search { get; set; }
}