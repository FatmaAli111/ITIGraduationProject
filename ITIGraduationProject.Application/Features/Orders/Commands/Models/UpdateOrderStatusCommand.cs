using ITIGraduationProject.Domain.Enums;
using MediatR;
using System.Text.Json.Serialization;

namespace ITIGraduationProject.Application.Features.Orders.Commands.Models
{
    public class UpdateOrderStatusCommand : IRequest<bool>
    {
        public Guid OrderId { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public OrderStatus NewStatus { get; set; }
    }
}
