using ITIGraduationProject.Application.Bases;
using ITIGraduationProject.Application.DTOS.Notification;
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
            Task<Response<bool>> SendNotificationAsync(
                Guid userId,
                string title,
                string message,
                NotificationType type);


            Task<Response<List<NotificationDto>>> GetNotificationsAsync(Guid userId);

            Task<Response<List<NotificationDto>>> GetUnreadNotificationsAsync(Guid userId);

            Task<Response<bool>> MarkAsReadAsync( Guid userId,Guid notificationId);

            Task<Response<bool>> MarkAllAsReadAsync(Guid userId);

          
        }
    
}
