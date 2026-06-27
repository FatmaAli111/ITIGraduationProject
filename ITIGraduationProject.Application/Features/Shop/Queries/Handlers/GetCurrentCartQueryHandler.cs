using System;
using System.Threading;
using System.Threading.Tasks;
using ITIGraduationProject.Application.Bases;
using ITIGraduationProject.Application.DTOS.ShopDTOs;
using ITIGraduationProject.Application.Features.Shop.Queries.Models;
using ITIGraduationProject.Application.Interfaces;
using ITIGraduationProject.Application.Interfaces.Persistence;
using MapsterMapper;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ITIGraduationProject.Application.Features.Shop.Queries.Handlers
{
    public class GetCurrentCartQueryHandler : ResponseHandler, IRequestHandler<GetCurrentCartQuery, Response<CartDto>>
    {
        private readonly IUnitOfWork _uow;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<GetCurrentCartQueryHandler> _logger;
        private readonly IMapper _mapper;

        public GetCurrentCartQueryHandler(
            IUnitOfWork uow,
            ICurrentUserService currentUserService,
            ILogger<GetCurrentCartQueryHandler> logger,
            IMapper mapper)
        {
            _uow = uow;
            _currentUserService = currentUserService;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<Response<CartDto>> Handle(GetCurrentCartQuery request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;
            _logger.LogInformation("Retrieving cart for authenticated user: {UserId}", userId);

            var cart = await _uow.Carts.GetCartWithItemsAsync(userId);

            if (cart == null)
            {
                _logger.LogInformation("No cart found for user {UserId}. Creating new shopping cart.", userId);
                cart = await _uow.Carts.CreateCartAsync(userId);
                await _uow.SaveChangesAsync();
                _logger.LogInformation("Cart created successfully with ID: {CartId} for user: {UserId}", cart.Id, userId);
            }

            var cartDto = _mapper.Map<CartDto>(cart);
            return Success(cartDto);
        }
    }
}
