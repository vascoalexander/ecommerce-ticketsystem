using System.ComponentModel.DataAnnotations;

namespace WebApp.Models;

public class Message
{
    public int Id { get; set; }
    [Required]
    public string SenderId { get; set; } = null!;
    [Required]
    public AppUser Sender { get; set; } = null!;

    [Required]
    public string ReceiverId { get; set; } = null!;
    [Required]
    public AppUser Receiver { get; set; } = null!;

    [StringLength(120, ErrorMessage = "Der Titel darf maximal 120 Zeichen lang sein.")]
    public string Subject { get; set; } = null!;
    [StringLength(2000, ErrorMessage = "Die Beschreibung darf maximal 2000 Zeichen lang sein.")]
    public string Body { get; set; } = null!;

    public DateTime SentAt { get; set; } = DateTime.UtcNow;
    public bool IsRead { get; set; }
}