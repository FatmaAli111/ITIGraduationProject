using ITIGraduationProject.Application.Bases;
using ITIGraduationProject.Application.DTOS.Notification;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITIGraduationProject.Application.Features.Notifications.Queries.Models
{
    public record GetUnreadNotificationsQuery()
      : IRequest<Response<List<NotificationDto>>>;
}
