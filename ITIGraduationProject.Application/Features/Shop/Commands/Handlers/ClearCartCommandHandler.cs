using System;
using System.Threading;
using System.Threading.Tasks;
using ITIGraduationProject.Application.Bases;
using ITIGraduationProject.Application.Features.Shop.Commands.Models;
using ITIGraduationProject.Application.Interfaces;
using ITIGraduationProject.Application.Interfaces.Persistence;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ITIGraduationProject.Application.Features.Shop.Commands.Handlers
{
    public class ClearCartCommandHandler : ResponseHandler, IRequestHandler<ClearCartCommand, Response<string>>
    {
        private readonly IUnitOfWork _uow;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<ClearCartCommandHandler> _logger;

        public ClearCartCommandHandler(
            IUnitOfWork uow,
            ICurrentUserService currentUserService,
            ILogger<ClearCartCommandHandler> logger)
        {
            _uow = uow;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<Response<string>> Handle(ClearCartCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;
            _logger.LogInformation("Clearing cart for User: {UserId}", userId);

            var cart = await _uow.Carts.GetCartWithItemsAsync(userId);
            if (cart == null)
            {
                _logger.LogWarning("No cart found to clear for user: {UserId}", userId);
                return Success("Cart was already empty.");
            }

            if (cart.CartItems.Any())
            {
                _logger.LogInformation("Removing {Count} item(s) from cart {CartId} for user {UserId}", cart.CartItems.Count, cart.Id, userId);
                cart.CartItems.Clear();
                await _uow.SaveChangesAsync();
                _logger.LogInformation("Cart cleared successfully.");
            }
            else
            {
                _logger.LogInformation("Cart is already empty.");
            }

            return Success("Cart cleared successfully.");
        }
    }
}
