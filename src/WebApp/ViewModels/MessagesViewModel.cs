using WebApp.Models;
namespace WebApp.ViewModels;

public class MessagesViewModel
{
    public IEnumerable<MessageDetailsViewModel> ReceivedMessages { get; set; } = new List<MessageDetailsViewModel>();
    public IEnumerable<MessageDetailsViewModel> SentMessages { get; set; } = new List<MessageDetailsViewModel>();
    public IEnumerable<MessageDetailsViewModel> SystemMessages { get; set; } = new List<MessageDetailsViewModel>();
}