namespace WebApp.Models;

public class TicketFile
{
    public int Id { get; set; }
    public required string OriginalName { get; set; }
    public required string StoredName { get; set; }
    public long FileSize { get; set; }
    public DateTime? UploadDate { get; set; }
    public required int TicketId { get; set; }
    public required TicketModel Ticket { get; set; }
}