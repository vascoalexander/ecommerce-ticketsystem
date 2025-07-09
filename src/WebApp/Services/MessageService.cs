using Microsoft.AspNetCore.Identity;
using WebApp.Models; // Ihre Domain-Modelle
using WebApp.Repositories;
using WebApp.ViewModels;

namespace WebApp.Services;

public class MessageService
{
    private readonly IMessageRepository _messageRepository;
    private readonly UserManager<AppUser> _userManager;

    public MessageService(IMessageRepository messageRepository, UserManager<AppUser> userManager)
    {
        _messageRepository = messageRepository;
        _userManager = userManager;
    }

    public async Task<MessagesViewModel> GetAllUserMessages(string userId)
    {
        var receivedMessagesTask = _messageRepository.GetMessagesReceived(userId);
        var sentMessagesTask = _messageRepository.GetMessagesSent(userId);
        var systemMessagesTask = _messageRepository.GetSystemMessageReceived(userId);

        await Task.WhenAll(receivedMessagesTask, sentMessagesTask, systemMessagesTask);

        var received = receivedMessagesTask.Result
            .Where(m => !m.IsDeletedReceiver)
            .Select(m => new MessageDetailsViewModel()
                {
                    Id = m.Id,
                    SenderName = m.Sender?.UserName ?? "Unbekannt",
                    SenderEmail = m.Sender?.Email ?? "Unbekannt",
                    ReceiverName = m.Receiver?.UserName ?? "Unbekannt",
                    ReceiverEmail = m.Receiver?.Email ?? "Unbekannt",
                    SentAt = m.SentAt,
                    Subject = m.Subject,
                    Body = m.Body,
                    IsRead = m.IsRead,
                }
            ).ToList();

        var sent = sentMessagesTask.Result
            .Where(m => !m.IsDeletedSender)
            .Select(m => new MessageDetailsViewModel()
            {
                Id = m.Id,
                SenderName = m.Sender?.UserName ?? "Unbekannt",
                SenderEmail = m.Sender?.Email ?? "Unbekannt",
                ReceiverName = m.Receiver?.UserName ?? "Unbekannt",
                ReceiverEmail = m.Receiver?.Email ?? "Unbekannt",
                SentAt = m.SentAt,
                Subject = m.Subject,
                Body = m.Body,
                IsRead = m.IsRead,
            }).ToList();

        var system = systemMessagesTask.Result
            .Where(m => !m.IsDeletedReceiver)
            .Select(m => new MessageDetailsViewModel()
            {
                Id = m.Id,
                SenderName = m.Sender?.UserName ?? "System",
                SentAt = m.SentAt,
                Subject = m.Subject,
                Body = m.Body,
                IsRead = m.IsRead,
            }).ToList();

        return new MessagesViewModel
        {
            ReceivedMessages = received,
            SentMessages = sent,
            SystemMessages = system
        };
    }
    public async Task<MessageDetailsViewModel> GetMessageDetails(int messageId)
    {
        var message = _messageRepository.GetByIdAsync(messageId).Result;

        return new MessageDetailsViewModel
        {
            Id = message.Id,


        };
    }
}