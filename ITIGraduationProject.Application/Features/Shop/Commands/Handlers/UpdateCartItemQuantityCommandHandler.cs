using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ITIGraduationProject.Application.Bases;
using ITIGraduationProject.Application.DTOS.ShopDTOs;
using ITIGraduationProject.Application.Features.Shop.Commands.Models;
using ITIGraduationProject.Application.Interfaces;
using ITIGraduationProject.Application.Interfaces.Persistence;
using MapsterMapper;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ITIGraduationProject.Application.Features.Shop.Commands.Handlers
{
    public class UpdateCartItemQuantityCommandHandler : ResponseHandler, IRequestHandler<UpdateCartItemQuantityCommand, Response<CartDto>>
    {
        private readonly IUnitOfWork _uow;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<UpdateCartItemQuantityCommandHandler> _logger;
        private readonly IMapper _mapper;

        public UpdateCartItemQuantityCommandHandler(
            IUnitOfWork uow,
            ICurrentUserService currentUserService,
            ILogger<UpdateCartItemQuantityCommandHandler> logger,
            IMapper mapper)
        {
            _uow = uow;
            _currentUserService = currentUserService;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<Response<CartDto>> Handle(UpdateCartItemQuantityCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;
            _logger.LogInformation("Updating quantity for CartItem {CartItemId} to {Quantity} by User: {UserId}",
                request.CartItemId, request.Quantity, userId);

            if (request.Quantity < 0)
            {
                _logger.LogWarning("Invalid negative quantity: {Quantity}", request.Quantity);
                return BadRequest<CartDto>("Quantity cannot be less than zero.");
            }

            var cart = await _uow.Carts.GetCartWithItemsAsync(userId);
            if (cart == null)
            {
                _logger.LogWarning("No cart found for user: {UserId}", userId);
                return NotFound<CartDto>("Cart not found.");
            }

            var item = cart.CartItems.FirstOrDefault(ci => ci.Id == request.CartItemId);
            if (item == null)
            {
                _logger.LogWarning("CartItem {CartItemId} not found in user {UserId}'s cart.", request.CartItemId, userId);
                return NotFound<CartDto>("Cart item not found in your cart.");
            }

            if (request.Quantity == 0)
            {
                _logger.LogInformation("Quantity is 0. Removing CartItem {CartItemId} from cart.", request.CartItemId);
                _uow.CartItems.Delete(item);
            }
            else
            {
                _logger.LogInformation("Updating CartItem {CartItemId} quantity from {OldQuantity} to {NewQuantity}",
                    request.CartItemId, item.Quantity, request.Quantity);
                item.Quantity = request.Quantity;
            }

            await _uow.SaveChangesAsync();
            _logger.LogInformation("Cart item quantity updated successfully.");

            // Reload full cart for updated prices/totals
            var updatedCart = await _uow.Carts.GetCartWithItemsAsync(userId);
            return Success(_mapper.Map<CartDto>(updatedCart ?? cart));
        }
    }
}
