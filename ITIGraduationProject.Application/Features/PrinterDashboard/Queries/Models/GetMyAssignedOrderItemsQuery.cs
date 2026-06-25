using ITIGraduationProject.Application.Bases;
using ITIGraduationProject.Application.DTOS.PrinterDashboard;
using ITIGraduationProject.Application.Wrapers;
using ITIGraduationProject.Domain.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITIGraduationProject.Application.Features.PrinterDashboard.Queries.Models
{
    public class GetMyAssignedOrderItemsQuery : IRequest<PaginatedResult<PrinterOrderItemDto>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public OrderItemStatus? Status { get; set; }
    }
}
