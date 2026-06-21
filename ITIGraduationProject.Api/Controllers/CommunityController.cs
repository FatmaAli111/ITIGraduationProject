using ITIGraduationProject.Application.Features.Community.Commands.Models;
using ITIGraduationProject.Application.Features.Community.Queries.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITIGraduationProject.Api.Controllers
{
    [Route("api/community")]
    [ApiController]
    public class CommunityController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CommunityController(IMediator mediator) => _mediator = mediator;

        [HttpGet("feed")]
        public async Task<IActionResult> GetFeed([FromQuery] GetCommunityFeedQuery query)
            => Ok(await _mediator.Send(query));

        [HttpPost("templates/{id:guid}/like")]
        [Authorize]
        public async Task<IActionResult> ToggleLike(Guid id)
            => Ok(await _mediator.Send(new ToggleLikeCommand(id)));

        [HttpPost("templates/{id:guid}/save")]
        [Authorize]
        public async Task<IActionResult> ToggleSave(Guid id)
            => Ok(await _mediator.Send(new ToggleSaveCommand(id)));

        [HttpGet("templates/{id:guid}/comments")]
        public async Task<IActionResult> GetComments(
            Guid id,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
            => Ok(await _mediator.Send(new GetTemplateCommentsQuery(id, pageNumber, pageSize)));

        [HttpPost("templates/{id:guid}/comments")]
        [Authorize]
        public async Task<IActionResult> AddComment(Guid id, [FromBody] AddCommentRequest body)
            => Ok(await _mediator.Send(new AddCommentCommand(id, body.Content)));

        [HttpDelete("comments/{id:guid}")]
        [Authorize]
        public async Task<IActionResult> DeleteComment(Guid id)
            => Ok(await _mediator.Send(new DeleteCommentCommand(id)));

        [HttpPost("templates/{id:guid}/report")]
        [Authorize]
        public async Task<IActionResult> ReportTemplate(Guid id, [FromBody] ReportTemplateRequest body)
            => Ok(await _mediator.Send(new ReportTemplateCommand(id, body.Reason)));

        [HttpGet("top-creators")]
        public async Task<IActionResult> GetTopCreators([FromQuery] GetTopCreatorsQuery query)
            => Ok(await _mediator.Send(query));
    }

    public record AddCommentRequest(string Content);
    public record ReportTemplateRequest(string Reason);
}
