using System.ComponentModel.DataAnnotations;

namespace WebApp.Models
{
    public class TicketHistoryModel
    {
        public int Id { get; set; }

        [Required]
        public int TicketId { get; set; }
        public required TicketModel Ticket { get; set; }

        [Required]
        [StringLength(100)]
        public string PropertyName { get; set; } = string.Empty;

        [StringLength(2000)]
        public string? OldValue { get; set; }

        [StringLength(2000)]
        public string? NewValue { get; set; }

        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

        public string? ChangedByUserId { get; set; }
        public AppUser? ChangedByUser { get; set; }
    }
}