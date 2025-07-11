namespace WebApp.ViewModels;

public class AdminUserManagementViewModel
{

    public List<AdminUserViewModel> Users { get; set; } = new();
    public string? Search { get; set; }
    public bool IncludeInactive { get; set; }


}