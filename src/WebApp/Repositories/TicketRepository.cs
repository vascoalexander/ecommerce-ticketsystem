using Microsoft.EntityFrameworkCore;
using WebApp.Data;
using WebApp.Models;

namespace WebApp.Repositories;

public class TicketRepository
{
    private readonly AppDbContext _context;

    public TicketRepository(AppDbContext context)
    {
        _context = context;
    }
    // Create 
    public async Task CreateTicketAsync(TicketModel ticket)
    {
        _context.Tickets.Add(ticket);
        await _context.SaveChangesAsync();
    }

    // Read 
    public async Task<TicketModel> GetTicketByIdAsync(int id)
    {
        var ticket = await _context.Tickets
            .Include(t => t.Project)
            .Include(t => t.AssignedUser)
            .Include(t => t.CreatorUser)
            .Include(t => t.Files)
            .FirstOrDefaultAsync(t => t.Id == id);
        if (ticket == null)
            throw new KeyNotFoundException($"Kein Ticket mit der ID {id} gefunden.");
        return ticket;
    }

    public async Task<List<TicketModel>> GetAllTicketsAsync()
    {
        return await _context.Tickets
            .Include(t => t.Project)
            .Include(t => t.AssignedUser)
            .Include(t => t.CreatorUser)
            .ToListAsync();
    }

    // Update 
    public async Task UpdateTicketAsync(TicketModel ticket)
    {
        _context.Tickets.Update(ticket);
        await _context.SaveChangesAsync();
    }

    public async Task<List<TicketModel>> GetTicketsForUserAsync(string userId)
    {
        return await _context.Tickets
            .Where(t => t.CreatorUserId == userId || t.AssignedUserId == userId)
            .Include(t => t.Project)
            .Include(t => t.AssignedUser)
            .Include(t => t.CreatorUser)
            .ToListAsync();
    }

    // Delete 
    public async Task DeleteTicketAsync(int id)
    {
        var ticket = await _context.Tickets.FindAsync(id);
        if (ticket != null)
        {
            _context.Tickets.Remove(ticket);
            await _context.SaveChangesAsync();
        }
    }
}