using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITIGraduationProject.Application.Interfaces.IServices.Notification
{
    public interface INotificationSender
    {
        Task SendToUserAsync(
            Guid userId,
            Guid notificationId,
            string title,
            string message,
            string type,
            bool isRead,
            DateTime createdAt);
    }
}
