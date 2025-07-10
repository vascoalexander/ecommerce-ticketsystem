using WebApp.Models;

namespace WebApp.Repositories;

public interface IMessageRepository
{
    Task<Message?> GetByIdAsync(int id);
    Task AddAsync(Message message);
    void UpdateAsync(Message message);
    Task<IEnumerable<Message>> GetMessagesReceived(string userId);
    Task<IEnumerable<Message>> GetSystemMessageReceived(string userId);
    Task<IEnumerable<Message>> GetMessagesSent(string userId);
    Task SaveChangesAsync();
}