using System.ComponentModel.DataAnnotations;

public enum TicketProperty
{
    Title,
    Description,
    Status,
    AssignedUser,
    Project
}

namespace WebApp.Models
{
    public class TicketHistoryModel
    {
        public int Id { get; set; }

        [Required]
        public int TicketId { get; set; }
        public TicketModel? Ticket { get; set; }

        [Required]
        public TicketProperty PropertyName { get; set; }

        [StringLength(2000)]
        public string? OldValue { get; set; }

        [StringLength(2000)]
        public string? NewValue { get; set; }

        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

        public string? ChangedByUserId { get; set; }
        public AppUser? ChangedByUser { get; set; }
    }
}