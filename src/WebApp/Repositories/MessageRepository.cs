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

    public async Task<Message?> GetMessageById(int id)
    {
        return (await _context.Messages
            .Include(m => m.Sender)
            .Include(m => m.Receiver)
            .FirstOrDefaultAsync(m => m.Id == id))!;
    }

    public async Task<IEnumerable<Message>> GetMessagesReceived(string userId)
    {
        return await _context.Messages
            .Where(m => m.ReceiverId ==  userId)
            .Include(m => m.Sender)
            .OrderByDescending(m => m.SentAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Message>> GetMessagesSent(string userId)
    {
        return await _context.Messages
            .Where(m => m.SenderId ==  userId)
            .Include(m => m.Receiver)
            .OrderByDescending(m => m.SentAt)
            .ToListAsync();
    }

    public async Task<int> GetUnreadMessages(string userId)
    {
        return await _context.Messages
            .Where(m => m.SenderId ==  userId && !m.IsRead)
            .CountAsync();
    }

    public async Task AddMessage(Message message)
    {
        await _context.Messages.AddAsync(message);
    }

    public void UpdateMessage(Message message)
    {
        _context.Messages.Update(message);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}