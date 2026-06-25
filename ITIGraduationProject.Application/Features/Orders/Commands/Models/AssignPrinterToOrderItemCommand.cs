using ITIGraduationProject.Application.Bases;
using ITIGraduationProject.Application.DTOS.Orders;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITIGraduationProject.Application.Features.Orders.Commands.Models
{
    public class AssignPrinterToOrderItemCommand : IRequest<Response<AssignPrinterToOrderItemDto>>
    {
        public Guid OrderItemId { get; set; }
        public Guid PrinterProfileId { get; set; }
    }
}
