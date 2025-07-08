using WebApp.Models;

namespace WebApp.Repositories;

public interface IMessageRepository : IRepository<Message, int>
{
    Task<IEnumerable<Message>> GetMessagesReceived(string userId);
    Task<IEnumerable<Message>> GetSystemMessageReceived(string userId);
    Task<IEnumerable<Message>> GetMessagesSent(string userId);
    Task<int> GetUnreadMessages(string userId);
}