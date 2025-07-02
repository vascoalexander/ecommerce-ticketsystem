using Microsoft.EntityFrameworkCore;
using WebApp.Data;
using WebApp.Models;

namespace WebApp.Repositories;

public class TicketHistoryRepository
{
    private readonly AppDbContext _context;

    public TicketHistoryRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<TicketHistoryModel>> GetHistoryForTicketAsync(int ticketId)
    {
        return await _context.Set<TicketHistoryModel>()
            .Include(h => h.ChangedByUser)
            .Where(h => h.TicketId == ticketId)
            .OrderByDescending(h => h.ChangedAt)
            .ToListAsync();
    }
    public void TrackChange(TicketModel ticket, TicketProperty property, string? oldVal, string? newVal, string? userId)
    {
        var history = new TicketHistoryModel
        {
            TicketId = ticket.Id,
            PropertyName = property, 
            OldValue = oldVal,
            NewValue = newVal,
            ChangedByUserId = userId,
            ChangedAt = DateTime.UtcNow
        };

        _context.TicketHistories.Add(history);
    }
    
    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}