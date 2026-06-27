using System;
using ITIGraduationProject.Application.Bases;
using ITIGraduationProject.Application.DTOS.ShopDTOs;
using MediatR;

namespace ITIGraduationProject.Application.Features.Shop.Commands.Models
{
    public record UpdateCartItemQuantityCommand(Guid CartItemId, int Quantity) : IRequest<Response<CartDto>>;
}
