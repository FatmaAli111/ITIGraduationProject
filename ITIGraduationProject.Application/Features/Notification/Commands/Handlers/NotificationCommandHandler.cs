using ITIGraduationProject.Application.Bases;
using ITIGraduationProject.Application.Features.Notification.Commands.Models;
using ITIGraduationProject.Application.Interfaces.IServices.Notification;
using ITIGraduationProject.Application.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITIGraduationProject.Application.Features.Notification.Commands.Handlers
{
    public class NotificationCommandHandler : ResponseHandler,
           IRequestHandler<MarkNotificationAsReadCommand, Response<bool>>,
           IRequestHandler<MarkAllNotificationsAsReadCommand, Response<bool>>
    {
        private readonly INotificationService _notificationService;
        private readonly ICurrentUserService _currentUserService;


        public NotificationCommandHandler(
            INotificationService notificationService,
            ICurrentUserService currentUserService)
        {
            _notificationService = notificationService;
            _currentUserService = currentUserService;
        }


        public async Task<Response<bool>> Handle(
            MarkNotificationAsReadCommand request,
            CancellationToken cancellationToken)
        {
            return await _notificationService.MarkAsReadAsync(
                _currentUserService.UserId,
                request.NotificationId);
        }


        public async Task<Response<bool>> Handle(
            MarkAllNotificationsAsReadCommand request,
            CancellationToken cancellationToken)
        {
            return await _notificationService.MarkAllAsReadAsync(
                _currentUserService.UserId);
        }
    }
}
