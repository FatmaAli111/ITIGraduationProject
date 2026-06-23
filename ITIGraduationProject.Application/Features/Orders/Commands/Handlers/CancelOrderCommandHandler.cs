using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using ITIGraduationProject.Application.Bases;
using ITIGraduationProject.Application.Features.Orders.Commands.Models;
using ITIGraduationProject.Application.Interfaces.Persistence;
using ITIGraduationProject.Domain.Entities.ECommerce;
using ITIGraduationProject.Domain.Enums;

namespace ITIGraduationProject.Application.Features.Orders.Commands.Handlers
{
    public class CancelOrderCommandHandler : ResponseHandler, IRequestHandler<CancelOrderCommand, Response<bool>>
    {
        #region Dependency Injection
        private readonly IUnitOfWork _unitOfWork;
        public CancelOrderCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;
        #endregion

        #region Handle Method
        public async Task<Response<bool>> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(request.OrderId);
            if (order == null) return NotFound<bool>("Order not found.");

            if (order.OrderStatus == OrderStatus.Shipped || order.OrderStatus == OrderStatus.Delivered)
            {
                return BadRequest<bool>("Cannot cancel an order that has already been shipped or delivered.");
            }

            order.OrderStatus = OrderStatus.Cancelled;
            _unitOfWork.Orders.Update(order);
            var result = await _unitOfWork.SaveChangesAsync();

            return result > 0 ? Success(true, "Order cancelled successfully.") : BadRequest<bool>("Failed to cancel order.");
        }
        #endregion
    }
}