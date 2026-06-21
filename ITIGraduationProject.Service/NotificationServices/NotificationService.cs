using ITIGraduationProject.Application.Bases;
using ITIGraduationProject.Application.DTOS.Notification;
using ITIGraduationProject.Application.Interfaces.IServices.Notification;
using ITIGraduationProject.Application.Interfaces.Persistence;
using ITIGraduationProject.Domain.Enums;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace ITIGraduationProject.Service.NotificationServices
{
    public class NotificationService :ResponseHandler, INotificationService
    {
        private readonly INotificationSender _notificationSender;
        private readonly IUnitOfWork _unitOfWork;

        public NotificationService(INotificationSender notificationSender,IUnitOfWork unitOfWork)
        {
            _notificationSender = notificationSender;
            _unitOfWork = unitOfWork;
        }



        public async Task<Response<List<NotificationDto>>> GetNotificationsAsync(Guid userId)
        {
            var notifications =
                await _unitOfWork.Notifications
                .GetByUserAsync(userId);


            var result = notifications.Adapt<List<NotificationDto>>();


            return Success(result);
        }


        public async Task<Response<List<NotificationDto>>> GetUnreadNotificationsAsync(Guid userId)
        {
            var notifications =
                await _unitOfWork.Notifications
                .GetUnreadByUserAsync(userId);


            var result = notifications.Adapt<List<NotificationDto>>();


            return Success(result);
        }


        public async Task<Response<bool>> MarkAsReadAsync(
            Guid userId,
            Guid notificationId)
        {
            var notification =
                await _unitOfWork.Notifications
                .GetByIdAsync(notificationId);


            if (notification == null)
                return NotFound<bool>("Notification not found");


            if (notification.UserId != userId)
                return Unauthorized<bool>();


            notification.IsRead = true;


            _unitOfWork.Notifications.Update(notification);


            await _unitOfWork.SaveChangesAsync();


            return Success(true);
        }


        public async Task<Response<bool>> MarkAllAsReadAsync(Guid userId)
        {
            var notifications =
                await _unitOfWork.Notifications
                .GetTableAsTracking()
                .Where(x =>
                    x.UserId == userId &&
                    !x.IsRead)
                .ToListAsync();


            foreach (var notification in notifications)
            {
                notification.IsRead = true;
            }


            await _unitOfWork.SaveChangesAsync();


            return Success(true);
        }
        public async Task<Response<bool>> SendNotificationAsync(
           Guid userId,
           string title,
           string message,
           NotificationType type)
        {
            var notification = new Domain.Entities.AIAndModeration.Notification
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


            return Success(true);
        }
    }
}
