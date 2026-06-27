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
    public class RemoveCartItemCommandHandler : ResponseHandler, IRequestHandler<RemoveCartItemCommand, Response<CartDto>>
    {
        private readonly IUnitOfWork _uow;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<RemoveCartItemCommandHandler> _logger;
        private readonly IMapper _mapper;

        public RemoveCartItemCommandHandler(
            IUnitOfWork uow,
            ICurrentUserService currentUserService,
            ILogger<RemoveCartItemCommandHandler> logger,
            IMapper mapper)
        {
            _uow = uow;
            _currentUserService = currentUserService;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<Response<CartDto>> Handle(RemoveCartItemCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;
            _logger.LogInformation("Removing CartItem {CartItemId} by User: {UserId}", request.CartItemId, userId);

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

            _logger.LogInformation("Removing CartItem {CartItemId} from cart {CartId}", request.CartItemId, cart.Id);
            _uow.CartItems.Delete(item);

            await _uow.SaveChangesAsync();
            _logger.LogInformation("Cart item removed successfully.");

            // Reload full cart
            var updatedCart = await _uow.Carts.GetCartWithItemsAsync(userId);
            return Success(_mapper.Map<CartDto>(updatedCart ?? cart));
        }
    }
}
