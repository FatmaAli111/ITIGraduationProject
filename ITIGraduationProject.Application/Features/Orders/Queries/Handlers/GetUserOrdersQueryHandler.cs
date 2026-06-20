using ITIGraduationProject.Application.Bases;
using ITIGraduationProject.Application.DTOS.Orders;
using ITIGraduationProject.Application.Features.Orders.Queries.Models;
using ITIGraduationProject.Application.Interfaces.Persistence;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ITIGraduationProject.Application.Features.Orders.Queries.Handlers
{
    public class GetUserOrdersQueryHandler : ResponseHandler, IRequestHandler<GetUserOrdersQuery, Response<List<OrderListDTO>>>
    {
        #region Dependency Injection
        private readonly IUnitOfWork _unitOfWork;
        public GetUserOrdersQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        #endregion

        #region Handle Method
        public async Task<Response<List<OrderListDTO>>> Handle(GetUserOrdersQuery request, CancellationToken cancellationToken) {

            var orders = await _unitOfWork.Orders.GetTableNoTracking()
                .Include(order => order.Shipment)
                .Include(order => order.OrderItems)
                .ThenInclude(order => order.Design)
                .Where(order => order.UserId == request.UserId)
                .OrderByDescending(order => order.CreatedAt)
                .ToListAsync(cancellationToken);

            #region Checking If Orders Exist
            if (orders == null || !orders.Any())
            {
                return Success(new List<OrderListDTO>(), "No orders found for this user.");
            }
            #endregion
            var ordersDTO = orders.Adapt<List<OrderListDTO>>();

            return Success(ordersDTO, "User orders retrieved successfully.");
        }
            
    }
        #endregion
}
