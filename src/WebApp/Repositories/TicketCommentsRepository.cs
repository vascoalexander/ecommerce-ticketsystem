using Microsoft.EntityFrameworkCore;
using WebApp.Data;
using WebApp.Models;

namespace WebApp.Repositories;

public class TicketCommentsRepository
{
    private readonly AppDbContext _context;

    public TicketCommentsRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<TicketComments>> GetAllCommentsForTicketAsync(int ticketId)
    {
        return await _context.Set<TicketComments>()
            .Include(c => c.CreatorUser)
            .Where(x => x.TicketId == ticketId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

    public void CreateComment(int ticketId, string content, string creatorUserId)
    {
        var comment = new TicketComments();
        {
            comment.TicketId = ticketId;
            comment.CreatedAt = DateTime.Now.ToUniversalTime();
            comment.Content = content;
            comment.CreatorUserId = creatorUserId;
        }

        _context.Add(comment);
    }

    public async Task SaveCommentAsync()
    {
        await _context.SaveChangesAsync();
    }
}