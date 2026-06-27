using ITIGraduationProject.Application.Interfaces.IServices.Notification;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITIGraduationProject.Infrastructure.SignalR
{
    public class SignalRNotificationSender : INotificationSender
    {
        private readonly IHubContext<NotificationHub> _hubContext;

        public SignalRNotificationSender(
            IHubContext<NotificationHub> hubContext)
        {
            _hubContext = hubContext;
        }


        public async Task SendToUserAsync(
            Guid userId,
            Guid notificationId,
            string title,
            string message,
            string type,
            bool isRead,
            DateTime createdAt)
        {
            await _hubContext
                .Clients
                .User(userId.ToString())
                .SendAsync(
                    "ReceiveNotification",
                    new
                    {
                        Id = notificationId,
                        Title = title,
                        Message = message,
                        Type = type,
                        IsRead = isRead,
                        CreatedAt = createdAt
                    });
        }
    }
}
