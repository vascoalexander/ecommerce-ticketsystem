using Microsoft.AspNetCore.Mvc.Rendering;
using WebApp.ViewModels;

namespace WebApp.Services;

public interface IMessageService
{
    Task<MessagesViewModel> GetAllUserMessages();
    Task<MessageDetailsViewModel> GetMessageDetails(int messageId);
    Task MarkMessageAsDeleted(int messageId);
    Task<SendMessageViewModel> PrepareSendMessageViewModelAsync(int? replyToMessageId);
    Task SendNewMessageAsync(SendMessageViewModel model);
    Task SendNewCommentNotificationAsync(string currentUserId, string currentUserName, string senderId, int ticketId,
        string commentContent);
    Task SendNewTicketAssignmentNotificationAsync(string assignedUserId, string senderId, int ticketId,
        DateTime ticketCreatedAt, string currentUserName);
    Task SendNewAssignmentNotificationAsync(string assignedUserId, string currentUserName, string senderId,
        int ticketId);
    Task<IEnumerable<SelectListItem>> GetAvailableReceiversAsync();
}