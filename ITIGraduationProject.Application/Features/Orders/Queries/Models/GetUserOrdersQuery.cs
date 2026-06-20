using MediatR;
using ITIGraduationProject.Application.Bases;
using ITIGraduationProject.Application.DTOS.Orders;

namespace ITIGraduationProject.Application.Features.Orders.Queries.Models
{
    public class GetUserOrdersQuery : IRequest<Response<List<OrderListDTO>>>
    {
        public Guid UserId { get; set; }
        public GetUserOrdersQuery(Guid userId)
        {
            UserId = userId;
        }
    }
}
