using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ITIGraduationProject.Application.Wrapers.Shop.CQRS;

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
        {
            return Ok(await _mediator.Send(query));
        }
        public async Task<IActionResult> GetProductById(
            [FromQuery] GetProductByIdQuery query, CancellationToken ct)
        {
            return Ok(await _mediator.Send(query));
        }
    }
}
