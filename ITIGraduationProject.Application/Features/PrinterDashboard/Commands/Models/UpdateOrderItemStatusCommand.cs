using ITIGraduationProject.Application.Bases;
using ITIGraduationProject.Application.DTOS.PrinterDashboard;
using ITIGraduationProject.Domain.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITIGraduationProject.Application.Features.PrinterDashboard.Commands.Models
{
    public class UpdateOrderItemStatusCommand : IRequest<Response<PrinterOrderItemDto>>
    {
        public Guid Id { get; set; }
        public OrderItemStatus NewStatus { get; set; }
    }
}
