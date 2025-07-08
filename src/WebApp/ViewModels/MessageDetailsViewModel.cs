namespace WebApp.ViewModels;

public class MessageDetailsViewModel
{
    public int Id { get; set; }

    public string SenderName { get; set; } = string.Empty;
    public string SenderEmail { get; set; } = string.Empty;
    public string ReceiverName { get; set; } = string.Empty;
    public string ReceiverEmail { get; set; } = string.Empty;

    public DateTime SentAt { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;

    public bool IsRead { get; set; }
    public bool IsDeletedSender { get; set; }
    public bool IsDeletedReceiver { get; set; }
}