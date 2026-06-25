using ITIGraduationProject.Application.Bases;
using ITIGraduationProject.Application.DTOS.Orders;
using ITIGraduationProject.Application.Features.Orders.Commands.Models;
using ITIGraduationProject.Application.Interfaces.Persistence;
using ITIGraduationProject.Domain.Entities.ECommerce;
using ITIGraduationProject.Domain.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITIGraduationProject.Application.Features.Orders.Commands.Handlers
{
    public class AssignPrinterToOrderItemCommandHandler : ResponseHandler, IRequestHandler<AssignPrinterToOrderItemCommand, Response<AssignPrinterToOrderItemDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public AssignPrinterToOrderItemCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Response<AssignPrinterToOrderItemDto>> Handle(AssignPrinterToOrderItemCommand request, CancellationToken cancellationToken)
        {
            var orderItem = await _unitOfWork.OrderItems.GetByIdAsync(request.OrderItemId);
            if (orderItem == null)
                return NotFound<AssignPrinterToOrderItemDto>("Order item not found.");

            var printerProfile = await _unitOfWork.PrinterProfiles.GetByIdAsync(request.PrinterProfileId);
            if (printerProfile == null || !printerProfile.IsActive)
                return BadRequest<AssignPrinterToOrderItemDto>("Printer profile not found or inactive.");

            orderItem.PrinterProfileId = request.PrinterProfileId;
            orderItem.Status = OrderItemStatus.AssignedToPrinter;

            _unitOfWork.OrderItems.Update(orderItem);
            await _unitOfWork.SaveChangesAsync();

            var dto = new AssignPrinterToOrderItemDto
            {
                OrderItemId = orderItem.Id,
                Status = orderItem.Status,
                PrinterProfileId = orderItem.PrinterProfileId.Value
            };

            return Success(dto, "Printer assigned to order item successfully.");
        }
    }
}
