using ITIGraduationProject.Application.Bases;
using ITIGraduationProject.Application.DTOS.Notification;
using ITIGraduationProject.Application.Interfaces.IServices.Notification;
using ITIGraduationProject.Application.Interfaces;
using MediatR;
using ITIGraduationProject.Application.Features.Notifications.Queries.Models;

namespace ITIGraduationProject.Application.Features.Notifications.Queries.Handlers
{
    public class NotificationQueryHandler : ResponseHandler,
          IRequestHandler<GetNotificationsQuery, Response<List<NotificationDto>>>,
          IRequestHandler<GetUnreadNotificationsQuery, Response<List<NotificationDto>>>
    {
        private readonly INotificationService _notificationService;
        private readonly ICurrentUserService _currentUserService;


        public NotificationQueryHandler(
            INotificationService notificationService,
            ICurrentUserService currentUserService)
        {
            _notificationService = notificationService;
            _currentUserService = currentUserService;
        }


        public async Task<Response<List<NotificationDto>>> Handle(
            GetNotificationsQuery request,
            CancellationToken cancellationToken)
        {
            return await _notificationService.GetNotificationsAsync(
                _currentUserService.UserId);
        }


        public async Task<Response<List<NotificationDto>>> Handle(
            GetUnreadNotificationsQuery request,
            CancellationToken cancellationToken)
        {
            return await _notificationService.GetUnreadNotificationsAsync(
                _currentUserService.UserId);
        }
    }
}
