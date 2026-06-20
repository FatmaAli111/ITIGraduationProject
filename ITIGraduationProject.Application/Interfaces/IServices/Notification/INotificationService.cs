using ITIGraduationProject.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITIGraduationProject.Application.Interfaces.IServices.Notification
{
    public interface INotificationService
    {
        public interface INotificationService
        {
            Task SendNotificationAsync(
                Guid userId,
                string title,
                string message,
                NotificationType type);
        }
    }
}
