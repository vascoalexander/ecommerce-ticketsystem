using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebApp.Models;
using WebApp.Repositories;
using WebApp.ViewModels;

namespace WebApp.Services;

public class MessageService : IMessageService
{
    private readonly IMessageRepository _messageRepository;
    private readonly IUserManagementService _userManagementService;

    public MessageService(IMessageRepository messageRepository, IUserManagementService userManagementService)
    {
        _messageRepository = messageRepository;
        _userManagementService = userManagementService;
    }

    public async Task<MessagesViewModel> GetAllUserMessages()
    {
        var currentUserId = await _userManagementService.GetCurrentUserIdAsync();
        if (currentUserId == null)
        {
            throw new UnauthorizedAccessException("Kein authentifizierter Benutzer gefunden");
        }

        var receivedMessagesTask = await _messageRepository.GetMessagesReceived(currentUserId);
        var sentMessagesTask = await _messageRepository.GetMessagesSent(currentUserId);
        var systemMessagesTask = await _messageRepository.GetSystemMessageReceived(currentUserId);

        var received = receivedMessagesTask
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

        var sent = sentMessagesTask
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

        var system = systemMessagesTask
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
    public async Task<MessageDetailsViewModel> GetMessageDetails(int messageId)
    {
        var message = await _messageRepository.GetByIdAsync(messageId);
        var currentUserId = await _userManagementService.GetCurrentUserIdAsync();

        if (currentUserId == null)
            throw new UnauthorizedAccessException("Kein authentifizierter Benutzer gefunden");
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
    public async Task MarkMessageAsDeleted(int messageId)
    {
        var message = await _messageRepository.GetByIdAsync(messageId);
        var currentUserId = await _userManagementService.GetCurrentUserIdAsync();

        if (currentUserId == null)
        {
            throw new UnauthorizedAccessException("Kein authentifizierter Benutzer gefunden");
        }

        if (message!.ReceiverId == currentUserId)
            message.IsDeletedReceiver = true;
        else if (message.SenderId == currentUserId)
            message.IsDeletedSender = true;

        _messageRepository.UpdateAsync(message);
        await _messageRepository.SaveChangesAsync();
    }

    public async Task<SendMessageViewModel> PrepareSendMessageViewModelAsync(int? replyToMessageId)
    {
        var model = new SendMessageViewModel();
        var currentUserId = await _userManagementService.GetCurrentUserIdAsync();
        if (currentUserId == null)
        {
            throw new UnauthorizedAccessException("Kein authentifizierter Benutzer gefunden");
        }

        model.AvailableReceivers = await GetAvailableReceiversAsync();

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

    public async Task SendNewMessageAsync(SendMessageViewModel model)
    {
        var senderId = await _userManagementService.GetCurrentUserIdAsync();
        if (senderId == null)
        {
            throw new UnauthorizedAccessException("Kein authentifizierter Benutzer gefunden");
        }

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

    public async Task SendNewCommentNotificationAsync(string currentUserId, string currentUserName, string senderId, int ticketId, string commentContent)
    {
        var message = new Message
        {
            SenderId = senderId,
            ReceiverId = currentUserId,
            SentAt = DateTime.UtcNow,
            Subject = $"Es wurde ein Neues Kommentar für das Ticket: {ticketId} erstellt",
            Body = $"""
                    Das Kommentar wurde vom Benutzer: {currentUserName} um {DateTime.Now.ToUniversalTime()} hinterlassen:
                    {commentContent}
                    """,
            IsRead = false,
            IsDeletedSender = false,
            IsDeletedReceiver = false,
        };
        await _messageRepository.AddAsync(message);
        await _messageRepository.SaveChangesAsync();
    }
    public async Task SendNewTicketAssignmentNotificationAsync(string assignedUserId, string senderId, int ticketId, DateTime ticketCreatedAt, string currentUserName)
    {
        var message = new Message
        {
            SenderId = senderId,
            ReceiverId = assignedUserId,
            SentAt = DateTime.Now.ToUniversalTime(),
            Subject = $"Ein Neues Ticket mit der ID: {ticketId} wurde erstellt",
            Body = $"Das Ticket wurde von {currentUserName} um {ticketCreatedAt} erstellt"

        };
        await _messageRepository.AddAsync(message);
        await _messageRepository.SaveChangesAsync();
    }
    public async Task SendNewAssignmentNotificationAsync(string assignedUserId, string currentUserName,string senderId, int ticketId)
    {
        var message = new Message
        {
            SenderId = senderId,
            ReceiverId = assignedUserId,
            SentAt = DateTime.UtcNow,
            Subject = $"Sie wurden dem Ticket mit der ID: {ticketId} für die bearbeitung zugewiesen",
            Body = $"Das Ticket wurde Ihnen zugewiesen von {currentUserName} um {DateTime.Now.ToUniversalTime()}",
            IsRead = false,
            IsDeletedSender = false,
            IsDeletedReceiver = false,
        };
        await _messageRepository.AddAsync(message);
        await _messageRepository.SaveChangesAsync();
    }
    public async Task<IEnumerable<SelectListItem>> GetAvailableReceiversAsync()
    {
        var currentUserId = await _userManagementService.GetCurrentUserIdAsync();
        return await _userManagementService.GetAllUsersQuery()
            .Where(u => u.Id != currentUserId)
            .OrderBy(u => u.UserName)
            .Select(u => new SelectListItem { Text = u.UserName, Value = u.Id.ToString() })
            .ToListAsync();
    }
}