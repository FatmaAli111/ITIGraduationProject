using ITIGraduationProject.Domain.Enums;
using MediatR;

namespace ITIGraduationProject.Application.Features.Orders.Commands.Models
{
    public class UpdateOrderStatusCommand : IRequest<bool>
    {
        public Guid OrderId { get; set; }
        public OrderStatus NewStatus { get; set; }
    }
}