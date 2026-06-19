using ITIGraduationProject.Application.Wrapers.Shop.CQRS;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ITIGraduationProject.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IMediator _mediator;
        public ProductsController(IMediator mediator)
        {
            _mediator = mediator;
        }
        [HttpGet("GetProducts")]
        public async Task<IActionResult> GetProducts(
            [FromQuery] GetProductsQuery query, CancellationToken ct)
        {
            return Ok(await _mediator.Send(query));
        }
        [HttpGet("/:{id}")]
        public async Task<IActionResult> GetProductById(
            [FromRoute] GetProductByIdQuery query, CancellationToken ct)
        {
            return Ok(await _mediator.Send(query));
        }

        // Admin only — same controller, just protected
        [HttpPost]
        [Authorize()]
        public async Task<IActionResult> Create([FromBody] CreateProductCommand cmd)
            => Ok(await _mediator.Send(cmd));
        [HttpPut("{id:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductCommand cmd)
        => Ok(await _mediator.Send(cmd with { Id = id }));

        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid id)
            => Ok(await _mediator.Send(new DeleteProductCommand(id)));
    }
}
