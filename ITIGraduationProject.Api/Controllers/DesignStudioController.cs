using ITIGraduationProject.Application.Features.Studio.Queries.GetProductForCustomization;
using ITIGraduationProject.Application.Features.Studio.Queries.GetStudioProducts;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ITIGraduationProject.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DesignStudioController : ControllerBase
    {
        private readonly ISender _mediator;

        public DesignStudioController(ISender mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("products")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<StudioProductListItemDto>))]
        public async Task<IActionResult> GetStudioProducts(CancellationToken cancellationToken)
        {
            var query = new GetStudioProductsQuery();
            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }

        [HttpGet("products/{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(StudioProductDetailDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetProductForCustomization([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            var query = new GetProductForCustomizationQuery(id);
            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }
    }
}
