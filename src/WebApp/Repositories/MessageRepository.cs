using Microsoft.EntityFrameworkCore;
using WebApp.Data;
using WebApp.Models;

namespace WebApp.Repositories;

public class MessageRepository
{
    private readonly AppDbContext _context;

    public MessageRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Message>> GetAllMessages()
    {
        return await _context.Messages.ToListAsync();
    }

    public async Task<Message?> GetMessageById(int id)
    {
        return await _context.Messages
            .Include(m => m.Sender)
            .Include(m => m.Receiver)
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task AddMessage(Message message)
    {
        await _context.Messages.AddAsync(message);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}