using System;
using System.Threading.Tasks;
using ITIGraduationProject.Application.Features.Shop.Commands.Models;
using ITIGraduationProject.Application.Features.Shop.Queries.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ITIGraduationProject.Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class CartController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CartController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> GetCurrentCart(CancellationToken ct)
        {
            var result = await _mediator.Send(new GetCurrentCartQuery(), ct);
            return StatusCode((int)result.StatusCode, result);
        }

        [HttpPost("items")]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartCommand command, CancellationToken ct)
        {
            var result = await _mediator.Send(command, ct);
            return StatusCode((int)result.StatusCode, result);
        }

        [HttpPut("items/{id:guid}")]
        public async Task<IActionResult> UpdateQuantity([FromRoute] Guid id, [FromBody] UpdateCartItemQuantityCommand command, CancellationToken ct)
        {
            var finalCommand = command;
            if (id != command.CartItemId)
            {
                finalCommand = command with { CartItemId = id };
            }
            var result = await _mediator.Send(finalCommand, ct);
            return StatusCode((int)result.StatusCode, result);
        }

        [HttpDelete("items/{id:guid}")]
        public async Task<IActionResult> RemoveItem([FromRoute] Guid id, CancellationToken ct)
        {
            var result = await _mediator.Send(new RemoveCartItemCommand(id), ct);
            return StatusCode((int)result.StatusCode, result);
        }

        [HttpDelete]
        public async Task<IActionResult> ClearCart(CancellationToken ct)
        {
            var result = await _mediator.Send(new ClearCartCommand(), ct);
            return StatusCode((int)result.StatusCode, result);
        }
    }
}
