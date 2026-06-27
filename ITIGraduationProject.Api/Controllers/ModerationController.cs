using ITIGraduationProject.Application.Features.Community.Commands.Models;
using ITIGraduationProject.Application.Features.Community.Queries.Models;
using ITIGraduationProject.Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;
using ITIGraduationProject.Domain.Enums;

namespace ITIGraduationProject.Api.Controllers
{
    [Route("api/admin/moderation")]
    [ApiController]
    [Authorize(Roles = Roles.Admin)]
    public class ModerationController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ModerationController(IMediator mediator) => _mediator = mediator;

        [HttpGet("reports")]
        public async Task<IActionResult> GetReports([FromQuery] GetModerationReportsQuery query)
            => Ok(await _mediator.Send(query));

        [HttpPatch("reports/{id:guid}")]
        public async Task<IActionResult> ResolveReport(Guid id, [FromBody] ResolveReportRequest body)
        {
            var result = await _mediator.Send(
                new ResolveModerationReportCommand(id, body.ActionTaken, body.Status));
            return StatusCode((int)result.StatusCode, result);
        }
    }

    public record ResolveReportRequest(
        string? ActionTaken,
        [property: JsonConverter(typeof(JsonStringEnumConverter))]
        ModerationReportStatus? Status = null);
}
