using ITIGraduationProject.Application.Interfaces.IServices.Notification;
using ITIGraduationProject.Application.Interfaces.Persistence;
using ITIGraduationProject.Application.Repositories;
using ITIGraduationProject.Domain.Entities.AIAndModeration;
using ITIGraduationProject.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITIGraduationProject.Service.NotificationServices
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationSender _notificationSender;
        private readonly IUnitOfWork _unitOfWork;

        public NotificationService(INotificationSender notificationSender,IUnitOfWork unitOfWork)
        {
            _notificationSender = notificationSender;
            _unitOfWork = unitOfWork;
        }


        public async Task SendNotificationAsync(
            Guid userId,
            string title,
            string message,
            NotificationType type)
        {
            var notification = new Notification
            {
                UserId = userId,
                Title = title,
                Message = message,
                Type = type,
                IsRead = false
            };


            await _unitOfWork.Notifications.AddAsync(notification);

            await _unitOfWork.SaveChangesAsync();


            await _notificationSender.SendToUserAsync(
                userId,
                title,
                message);
        }
    }
}
