using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ITIGraduationProject.Application.Bases;
using ITIGraduationProject.Application.DTOS.ShopDTOs;
using ITIGraduationProject.Application.Features.Shop.Commands.Models;
using ITIGraduationProject.Application.Interfaces;
using ITIGraduationProject.Application.Interfaces.Persistence;
using ITIGraduationProject.Domain.Entities.ECommerce;
using MapsterMapper;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ITIGraduationProject.Application.Features.Shop.Commands.Handlers
{
    public class AddToCartCommandHandler : ResponseHandler, IRequestHandler<AddToCartCommand, Response<CartDto>>
    {
        private readonly IUnitOfWork _uow;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<AddToCartCommandHandler> _logger;
        private readonly IMapper _mapper;

        public AddToCartCommandHandler(
            IUnitOfWork uow,
            ICurrentUserService currentUserService,
            ILogger<AddToCartCommandHandler> logger,
            IMapper mapper)
        {
            _uow = uow;
            _currentUserService = currentUserService;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<Response<CartDto>> Handle(AddToCartCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;
            _logger.LogInformation("Add to cart requested. ProductId: {ProductId}, DesignId: {DesignId}, Quantity: {Quantity} by User: {UserId}",
                request.ProductId, request.DesignId, request.Quantity, userId);

            if (request.Quantity <= 0)
            {
                _logger.LogWarning("Invalid quantity: {Quantity}. Must be greater than 0.", request.Quantity);
                return BadRequest<CartDto>("Quantity must be greater than zero.");
            }

            // 1. Load user's cart
            var cart = await _uow.Carts.GetCartWithItemsAsync(userId);
            if (cart == null)
            {
                _logger.LogInformation("No cart found for user {UserId}. Creating new shopping cart.", userId);
                cart = await _uow.Carts.CreateCartAsync(userId);
                await _uow.SaveChangesAsync();
            }

            // 2. Validate Product exists
            var product = await _uow.Products.GetByIdAsync(request.ProductId);
            if (product == null)
            {
                _logger.LogWarning("Product with ID {ProductId} was not found.", request.ProductId);
                return BadRequest<CartDto>("Product not found.");
            }

            Guid? validDesignId = null;
            string snapshotImageUrl = string.Empty;

            // 3. Validate Design exists and belongs to the user if supplied
            if (request.DesignId.HasValue && request.DesignId.Value != Guid.Empty)
            {
                var design = await _uow.Designs.GetByIdAsync(request.DesignId.Value);
                if (design == null)
                {
                    _logger.LogWarning("Design with ID {DesignId} was not found.", request.DesignId.Value);
                    return BadRequest<CartDto>("Design not found.");
                }

                if (design.UserId != userId)
                {
                    _logger.LogWarning("User {UserId} attempted to add design {DesignId} belonging to user {OwnerId}",
                        userId, design.Id, design.UserId);
                    return BadRequest<CartDto>("Unauthorized: This design does not belong to you.");
                }

                validDesignId = design.Id;
                snapshotImageUrl = design.SnapshotImageURL;
            }

            // 4. Search for existing item
            CartItem? existingItem = null;
            if (validDesignId == null)
            {
                existingItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == request.ProductId && ci.DesignId == null);
            }
            else
            {
                existingItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == request.ProductId && ci.DesignId == validDesignId);
            }

            if (existingItem != null)
            {
                _logger.LogInformation("Existing cart item found. Incrementing quantity from {OldQuantity} by {AddQuantity}",
                    existingItem.Quantity, request.Quantity);
                existingItem.Quantity += request.Quantity;
            }
            else
            {
                _logger.LogInformation("Creating new cart item for ProductId: {ProductId}, DesignId: {DesignId}, Quantity: {Quantity}",
                    request.ProductId, validDesignId, request.Quantity);

                var newItem = new CartItem
                {
                    CartId = cart.Id,
                    ProductId = request.ProductId,
                    DesignId = validDesignId,
                    Quantity = request.Quantity,
                    UnitPrice = product.BasePrice,
                    SnapshotImageUrl = snapshotImageUrl,
                    CreatedAt = DateTime.UtcNow
                };

                await _uow.CartItems.AddAsync(newItem);
            }

            await _uow.SaveChangesAsync();
            _logger.LogInformation("Shopping cart updated successfully for user {UserId}.", userId);

            // Fetch the fully populated cart to return correct DTO mappings
            var updatedCart = await _uow.Carts.GetCartWithItemsAsync(userId);
            return Success(_mapper.Map<CartDto>(updatedCart ?? cart));
        }
    }
}
