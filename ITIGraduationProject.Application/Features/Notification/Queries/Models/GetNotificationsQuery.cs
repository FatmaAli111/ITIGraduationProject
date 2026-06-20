using ITIGraduationProject.Application.Features.Notification.Queries.DTOS;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITIGraduationProject.Application.Features.Notification.Queries.Models
{
    public record GetNotificationsQuery()
     : IRequest<List<NotificationDto>>;
}
