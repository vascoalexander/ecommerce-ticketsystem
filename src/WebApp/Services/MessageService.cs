using WebApp.Models;
using WebApp.Repositories;
using WebApp.ViewModels;

namespace WebApp.Services;

public class MessageService
{
    private readonly IMessageRepository _messageRepository;
    private readonly IUserManagementService _userManagementService;

    public MessageService(IMessageRepository messageRepository, IUserManagementService userManagementService)
    {
        _messageRepository = messageRepository;
        _userManagementService = userManagementService;
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
                    SenderName = m.Sender.UserName ?? "Unbekannt",
                    SenderEmail = m.Sender.Email ?? "Unbekannt",
                    ReceiverName = m.Receiver.UserName ?? "Unbekannt",
                    ReceiverEmail = m.Receiver.Email ?? "Unbekannt",
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
                SenderName = m.Sender.UserName ?? "Unbekannt",
                SenderEmail = m.Sender.Email ?? "Unbekannt",
                ReceiverName = m.Receiver.UserName ?? "Unbekannt",
                ReceiverEmail = m.Receiver.Email ?? "Unbekannt",
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
                SenderName = m.Sender.UserName ?? "System",
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
    public async Task<MessageDetailsViewModel> GetMessageDetails(int messageId, string currentUserId)
    {
        var message = await _messageRepository.GetByIdAsync(messageId);

        if (message == null)
            throw new KeyNotFoundException("Die angeforderte Nachricht wurde nicht gefunden.");

        if (message.SenderId != currentUserId && message.ReceiverId != currentUserId)
            throw new UnauthorizedAccessException("Sie sind nicht berechtigt, diese Nachricht anzuzeigen.");

        if (!message.IsRead && message.ReceiverId == currentUserId)
        {
            message.IsRead = true;
            _messageRepository.UpdateAsync(message);
            await _messageRepository.SaveChangesAsync();
        }

        return new MessageDetailsViewModel
        {
            Id = message.Id,
            SenderName = message.Sender.UserName ?? "Unbekannt",
            SenderEmail = message.Sender.Email ?? "Unbekannt",
            ReceiverName = message.Receiver.UserName ?? "Unbekannt",
            ReceiverEmail = message.Receiver.Email ?? "Unbekannt",
            SentAt = message.SentAt,
            Subject = message.Subject,
            Body = message.Body,
            IsRead = message.IsRead,
            IsDeletedSender = message.IsDeletedSender,
            IsDeletedReceiver = message.IsDeletedReceiver,
        };
    }
    public async Task MarkMessageAsDeleted(int messageId, string currentUserId)
    {
        var message = await _messageRepository.GetByIdAsync(messageId);

        if (message!.ReceiverId == currentUserId)
            message.IsDeletedReceiver = true;
        else if (message.SenderId == currentUserId)
            message.IsDeletedSender = true;

        _messageRepository.UpdateAsync(message);
        await _messageRepository.SaveChangesAsync();
    }

    public async Task<SendMessageViewModel> PrepareSendMessageViewModelAsync(string currentUserId, int? replyToMessageId)
    {
        var model = new SendMessageViewModel();
        model.AvailableReceivers = await _userManagementService.GetAvailableReceiversAsync(currentUserId);

        if (replyToMessageId.HasValue)
        {
            var originalMessage = await _messageRepository.GetByIdAsync(replyToMessageId.Value);
            if (originalMessage != null && originalMessage.ReceiverId == currentUserId)
            {
                model.ReceiverId = originalMessage.SenderId;
                if (!string.IsNullOrEmpty(originalMessage.Subject) && !originalMessage.Subject.StartsWith("Re: "))
                {
                    model.Subject = $"Re: {originalMessage.Subject}";
                }
                else if (!string.IsNullOrEmpty(originalMessage.Subject))
                {
                    model.Subject = originalMessage.Subject;
                }
            }
        }
        return model;
    }

    public async Task<SendMessageViewModel> ReplyToSender(string currentUserId, int receiverId)
    {
        var model = new SendMessageViewModel();
        model.AvailableReceivers = await _userManagementService.GetAvailableReceiversAsync(currentUserId);

        var originalMessage = await _messageRepository.GetByIdAsync(receiverId);
        if (originalMessage != null && originalMessage.ReceiverId == currentUserId)
        {
            model.ReceiverId = originalMessage.SenderId;
            if (!string.IsNullOrEmpty(originalMessage.Subject) && !originalMessage.Subject.StartsWith("Re: "))
                model.Subject = $"Re: {originalMessage.Subject}";
            else if (!string.IsNullOrEmpty(originalMessage.Subject))
                model.Subject = originalMessage.Subject;
        }
        return model;
    }

    public async Task SendNewMessageAsync(SendMessageViewModel model, string senderId)
    {
        if (string.IsNullOrWhiteSpace(model.Subject) || string.IsNullOrWhiteSpace(model.Body))
        {
            throw new ArgumentException("Betreff und Nachrichtentext dürfen nicht leer sein.");
        }

        var receiverUser = await _userManagementService.GetUserByIdAsync(model.ReceiverId);
        if (receiverUser == null)
        {
            throw new ArgumentException("Der ausgewählte Empfänger existiert nicht.");
        }

        var message = new Message
        {
            SenderId = senderId,
            ReceiverId = model.ReceiverId,
            Subject = model.Subject,
            Body = model.Body,
            SentAt = DateTime.UtcNow,
            IsRead = false,
            IsDeletedSender = false,
            IsDeletedReceiver = false,
        };

        await _messageRepository.AddAsync(message);
        await _messageRepository.SaveChangesAsync();
    }
}