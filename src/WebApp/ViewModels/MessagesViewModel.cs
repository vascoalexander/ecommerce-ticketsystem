using WebApp.Models;
namespace WebApp.ViewModels;

public class MessagesViewModel
{
    public IEnumerable<Message> ReceivedMessages { get; set; } = new List<Message>();
    public IEnumerable<Message> SentMessages { get; set; } = new List<Message>();
    
    public IEnumerable<Message> SystemMessages { get; set; } = new List<Message>();
}