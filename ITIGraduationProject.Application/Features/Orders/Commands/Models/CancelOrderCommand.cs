using MediatR;
using System;
using ITIGraduationProject.Application.Bases;

namespace ITIGraduationProject.Application.Features.Orders.Commands.Models
{
    public record CancelOrderCommand(Guid OrderId) : IRequest<Response<bool>>;
}