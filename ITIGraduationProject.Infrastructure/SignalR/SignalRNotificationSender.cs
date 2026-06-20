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
            string title,
            string message)
        {
            await _hubContext
                .Clients
                .User(userId.ToString())
                .SendAsync(
                    "ReceiveNotification",
                    new
                    {
                        Title = title,
                        Message = message
                    });
        }
    }
}
