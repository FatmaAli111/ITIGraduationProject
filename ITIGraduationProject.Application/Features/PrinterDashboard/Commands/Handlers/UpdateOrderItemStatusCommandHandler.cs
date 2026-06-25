using ITIGraduationProject.Application.Bases;
using ITIGraduationProject.Application.DTOS.PrinterDashboard;
using ITIGraduationProject.Application.Features.PrinterDashboard.Commands.Models;
using ITIGraduationProject.Application.Interfaces;
using ITIGraduationProject.Application.Interfaces.Persistence;
using ITIGraduationProject.Domain.Entities.ECommerce;
using ITIGraduationProject.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITIGraduationProject.Application.Features.PrinterDashboard.Commands.Handlers
{
    public class UpdateOrderItemStatusCommandHandler : ResponseHandler, IRequestHandler<UpdateOrderItemStatusCommand, Response<PrinterOrderItemDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;

        public UpdateOrderItemStatusCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
        {
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
        }

        public async Task<Response<PrinterOrderItemDto>> Handle(UpdateOrderItemStatusCommand request, CancellationToken cancellationToken)
        {
            var printerProfile = await _unitOfWork.PrinterProfiles
                .GetTableNoTracking()
                .FirstOrDefaultAsync(p => p.UserId == _currentUser.UserId);

            if (printerProfile == null)
                return NotFound<PrinterOrderItemDto>("No printer profile found for this user. Create one first.");

            var orderItem = await _unitOfWork.OrderItems.GetByIdAsync(request.Id);
            if (orderItem == null)
                return NotFound<PrinterOrderItemDto>("Order item not found.");

            if (orderItem.PrinterProfileId == null)
                return BadRequest<PrinterOrderItemDto>("This order item has not been assigned to a printer yet.");

            if (orderItem.PrinterProfileId != printerProfile.Id)
            {
                var response = Unauthorized<PrinterOrderItemDto>();
                response.Message = "You can only update order items assigned to you.";
                return response;
            }

            // Basic forward transition guardrail
            var currentStatusValue = (int)orderItem.Status;
            var newStatusValue = (int)request.NewStatus;
            if (newStatusValue < currentStatusValue)
                return BadRequest<PrinterOrderItemDto>("Cannot revert to a previous status.");

            orderItem.Status = request.NewStatus;
            _unitOfWork.OrderItems.Update(orderItem);
            await _unitOfWork.SaveChangesAsync();

            var order = await _unitOfWork.Orders.GetByIdAsync(orderItem.OrderId);

            var dto = new PrinterOrderItemDto
            {
                Id = orderItem.Id,
                OrderId = orderItem.OrderId,
                OrderNumber = order?.OrderNumber ?? string.Empty,
                DesignSnapshotUrl = orderItem.SnapshotImageURL,
                Quantity = orderItem.Quantity,
                Status = orderItem.Status.ToString()
            };

            return Success(dto);
        }
    }
}
