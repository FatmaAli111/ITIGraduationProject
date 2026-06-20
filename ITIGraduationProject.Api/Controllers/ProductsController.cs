using ITIGraduationProject.Application.Features.Shop.Commands.Models;
using ITIGraduationProject.Application.Features.Shop.Queries.Models;
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

        [HttpGet]
        public async Task<IActionResult> GetProducts(
            [FromQuery] GetProductsQuery query, CancellationToken ct)
            => Ok(await _mediator.Send(query, ct));

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetProductById(Guid id, CancellationToken ct)
            => Ok(await _mediator.Send(new GetProductByIdQuery(id), ct));

        // Admin only — same controller, just protected

        [HttpPost]
        [Authorize(Roles = "Admin")]
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