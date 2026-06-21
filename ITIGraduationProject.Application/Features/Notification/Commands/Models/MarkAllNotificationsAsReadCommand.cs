using ITIGraduationProject.Application.Bases;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITIGraduationProject.Application.Features.Notification.Commands.Models
{
    public record MarkAllNotificationsAsReadCommand()
      : IRequest<Response<bool>>;
}
