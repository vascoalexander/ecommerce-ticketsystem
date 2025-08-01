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
    public async Task CreateTicketAsync(TicketModel ticket)
    {
        _context.Tickets.Add(ticket);
        await _context.SaveChangesAsync();
    }

    public async Task<TicketModel?> GetTicketByIdAsync(int? id)
    {
        return await _context.Tickets
            .Include(t => t.Project)
            .Include(t => t.AssignedUser)
            .Include(t => t.CreatorUser)
            .Include(t => t.Files)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<List<TicketModel>> GetAllTicketsAsync()
    {
        return await _context.Tickets
            .Include(t => t.Project)
            .Include(t => t.AssignedUser)
            .Include(t => t.CreatorUser)
            .ToListAsync();
    }
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
}