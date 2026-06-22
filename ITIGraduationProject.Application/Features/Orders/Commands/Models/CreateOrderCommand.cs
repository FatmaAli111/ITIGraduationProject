using MediatR;
using ITIGraduationProject.Application.Bases;
using ITIGraduationProject.Application.DTOS.Orders;

namespace ITIGraduationProject.Application.Features.Orders.Commands.Models
{
    public class CreateOrderCommand : IRequest<Response<string>>
    {
        public Guid UserId { get; set; } 
        public string ReceiverName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string? DeliveryNotes { get; set; }
        public string? CouponCode { get; set; } 
        public List<CreateOrderItemDTO> OrderItems { get; set; } = new();
    }
}
