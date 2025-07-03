namespace WebApp.ViewModels;

public class UploadViewModel
{
    public int TicketId { get; set; }
    public required IFormFile? UploadedFile { get; set; }
}