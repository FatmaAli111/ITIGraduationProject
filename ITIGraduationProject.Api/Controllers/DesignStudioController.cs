using ITIGraduationProject.Application.CQRS.Commands;
using ITIGraduationProject.Application.Features.Studio.Commands.CreateDesign;
using ITIGraduationProject.Application.Features.Studio.Commands.DeleteDesign;
using ITIGraduationProject.Application.Features.Studio.Commands.UpdateDesign;
using ITIGraduationProject.Application.Features.Studio.Queries.GetDesignById;
using ITIGraduationProject.Application.Features.Studio.Queries.GetProductForCustomization;
using ITIGraduationProject.Application.Features.Studio.Queries.GetStudioProducts;
using ITIGraduationProject.Application.Features.Studio.Queries.GetUserDesigns;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ITIGraduationProject.Application.Features.Studio.Queries.GetAiChatMessages;
using ITIGraduationProject.Application.Features.Studio.Queries.GetUserAiChatSessions;
using ITIGraduationProject.Application.Features.Studio.Commands.CreateAIChatSession;
using ITIGraduationProject.Application.Features.Studio.Commands.CreateAIChatMessage;

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

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(Guid))]
        public async Task<IActionResult> CreateDesign([FromBody] CreateDesignCommand command, CancellationToken cancellationToken)
        {
            var designId = await _mediator.Send(command, cancellationToken);
            return CreatedAtAction(nameof(CreateDesign), new { id = designId }, designId);
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateDesign([FromRoute] Guid id, [FromBody] UpdateDesignCommand command, CancellationToken cancellationToken)
        {
            if (id != command.Id)
            {
                return BadRequest("Mismatch between route id and body id.");
            }

            await _mediator.Send(command, cancellationToken);
            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteDesign([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            var command = new DeleteDesignCommand(id);
            await _mediator.Send(command, cancellationToken);
            return NoContent();
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(DesignResponseDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetDesignById([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            var query = new GetDesignByIdQuery(id);
            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }

        [HttpGet("user/{userId:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<DesignResponseDto>))]
        public async Task<IActionResult> GetUserDesigns([FromRoute] Guid userId, CancellationToken cancellationToken)
        {
            var query = new GetUserDesignsQuery(userId);
            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }    

        [HttpPost("/api/ai-chat/sessions")]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(Guid))]
        public async Task<IActionResult> CreateAiChatSession([FromBody] CreateAiChatSessionCommand command, CancellationToken cancellationToken)
        {
            var id = await _mediator.Send(command, cancellationToken);
            return CreatedAtAction(nameof(CreateAiChatSession), new { id }, id);
        }

        [HttpPost("/api/ai-chat/messages")]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(Guid))]
        public async Task<IActionResult> CreateAiChatMessage([FromBody] CreateAiChatMessageCommand command, CancellationToken cancellationToken)
        {
            var id = await _mediator.Send(command, cancellationToken);
            return CreatedAtAction(nameof(CreateAiChatMessage), new { id }, id);
        }

        [HttpGet("/api/ai-chat/sessions/{sessionId:guid}/messages")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<AiChatMessageDto>))]
        public async Task<IActionResult> GetAiChatMessages([FromRoute] Guid sessionId, CancellationToken cancellationToken)
        {
            var query = new GetAiChatMessagesQuery(sessionId);
            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }

        [HttpGet("/api/ai-chat/user/{userId:guid}/sessions")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<ITIGraduationProject.Application.Features.Studio.Queries.GetUserAiChatSessions.AiChatSessionDto>))]
        public async Task<IActionResult> GetUserAiChatSessions([FromRoute] Guid userId, CancellationToken cancellationToken)
        {
            var query = new GetUserAiChatSessionsQuery(userId);
            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }

    }
}
